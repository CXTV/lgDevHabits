using System.Dynamic;
using System.Net.Mime;
using Asp.Versioning;
using FluentValidation;
using lgDevHabit.Api.Database;
using lgDevHabit.Api.DTOs.Common;
using lgDevHabit.Api.DTOs.Habits;
using lgDevHabit.Api.Entities;
using lgDevHabit.Api.Services;
using lgDevHabit.Api.Services.Sorting;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
#pragma warning disable CA1862

namespace lgDevHabit.Api.Controllers;


[ApiController]
[Route("habits")]
[ApiVersion(1.0)]
[Produces(
    MediaTypeNames.Application.Json,
    CustomMediaTypeNames.Application.JsonV1,
    CustomMediaTypeNames.Application.JsonV2,
    CustomMediaTypeNames.Application.HateoasJson,
    CustomMediaTypeNames.Application.HateoasJsonV1,
    CustomMediaTypeNames.Application.HateoasJsonV2)]
public sealed class HabitsController(
    ApplicationDbContext dbContext, 
    LinkService linkService,
    UserContext userContext
    ) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginationResult<HabitDto>>> GetHabits(
        [FromQuery] HabitsQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataShapingService dataShapingService
        )
    {
        //获取当前用户
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        //验证Sort参数
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid: '{query.Sort}'");
        }
        //验证DataShaping参数
        if (!dataShapingService.Validate<HabitDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{query.Fields}'");
        }

        query.Search = query.Search?.Trim().ToLower();

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        IQueryable<HabitDto> habitsQuery = dbContext
            .Habits
            //获取userID
            .Where(h => h.UserId == userId)
            //只能通过Name和Description进行模糊查询
            .Where(h => query.Search == null ||
                        h.Name.ToLower().Contains(query.Search) ||
                        h.Description != null && h.Description.ToLower().Contains(query.Search))
            .Where(h => query.Type == null || h.Type == query.Type)
            .Where(h => query.Status == null || h.Status == query.Status)
            //动态排序
            .ApplySort(query.Sort, sortMappings)
            .Select(HabitQueries.ProjectToDto());

        int totalCount = await habitsQuery.CountAsync();

        List<HabitDto> habits = await habitsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        //判断是否含有Accept头部
        bool includeLinks = query.Accept == CustomMediaTypeNames.Application.HateoasJson;

        //修改为返回DataShaping的数据
        var paginationResult = new PaginationResult<ExpandoObject>
        {
            //Items = dataShapingService.ShapeCollectionData(habits, query.Fields),
            Items = dataShapingService.ShapeCollectionData(
                habits, 
                query.Fields,
                                //h=> CreateLinksForHabit(h.Id,query.Fields)),
                h => includeLinks ? CreateLinksForHabit(h.Id, query.Fields) : null),

            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };

        if (includeLinks)
        {
            paginationResult.Links = CreateLinksForHabits(query, paginationResult.HasNextPage, paginationResult.HasPreviousPage);
        }

        return Ok(paginationResult);
    }
     
    //[HttpGet("{id}")]
    //public async Task<ActionResult<HabitDto?>> GetHabit(string id)
    //{
    //    HabitDto? habit = await dbContext
    //        .Habits
    //        .Where(h => h.Id == id)
    //        .Select(HabitQueries.ProjectToDto())
    //        .FirstOrDefaultAsync();

    //    if (habit is null)
    //    {
    //        return NotFound();
    //    }

    //    return Ok(habit);
    //}

    [HttpGet("{id}")]
    [ApiVersion(1.0)]
    public async Task<ActionResult<HabitWithTagsDto>> GetHabit(
        string id,
        string? fields,
        [FromHeader(Name = "Accept")]
        string? accept,
        DataShapingService dataShapingService
        )
    {

        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        //验证DataShaping参数
        if (!dataShapingService.Validate<HabitDto>(fields))
        {
            return Problem(
            statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{fields}'");
        }

        HabitWithTagsDto? habit = await dbContext
            .Habits
            .Where(h => h.Id == id && h.UserId == userId)
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDtoWithTags())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        //使用DataShapingService进行数据处理
        ExpandoObject shapedHabitDto = dataShapingService.ShapeData(habit, fields);

        if (accept == CustomMediaTypeNames.Application.HateoasJsonV1)
        {
            List<LinkDto> links = CreateLinksForHabit(id, fields);
            shapedHabitDto.TryAdd("linksV1", links);
        }


        return Ok(shapedHabitDto);
    }


    [HttpGet]
    [Route("{id}")]
    [ApiVersion(2.0)]
    public async Task<IActionResult> GetHabitV2(
        string id,
        string? fields,
        [FromHeader(Name = "Accept")]
        string? accept,
        DataShapingService dataShapingService)
    {

        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }


        if (!dataShapingService.Validate<HabitWithTagsDtoV2>(fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields aren't valid: '{fields}'");
        }

        HabitWithTagsDtoV2 habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Where(h => h.Id == id && h.UserId == userId)

            .Select(HabitQueries.ProjectToDtoWithTagsV2())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        ExpandoObject shapedHabitDto = dataShapingService.ShapeData(habit, fields);

        if (accept == CustomMediaTypeNames.Application.HateoasJsonV2)
        {
            List<LinkDto> links = CreateLinksForHabit(id, fields);
            shapedHabitDto.TryAdd("linksV2", links);
        }

        return Ok(shapedHabitDto);
    }


    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(
        CreateHabitDto createhabitDto,
        IValidator<CreateHabitDto> validator
        )
    {

        //ValidationResult validationResult = await validator.ValidateAsync(createhabitDto);

        //if (!validationResult.IsValid)
        //{

        //    return BadRequest(validationResult.ToDictionary());
        //}
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }


        await validator.ValidateAndThrowAsync(createhabitDto);

        Habit habit = createhabitDto.ToEntity(userId);

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        HabitDto habitDto = habit.ToDto();

        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);

    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {

        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }



    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {

        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (habit is null)
        {
            return NotFound();
        }

        HabitDto habitDto = habit.ToDto();

        patchDocument.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {

        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (habit is null)
        {
            return NotFound();
        }

        dbContext.Habits.Remove(habit);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    //添加Habit的公共Link方法
    private List<LinkDto> CreateLinksForHabit(string id, string? fields)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetHabit), "self", HttpMethods.Get, new { id,fields}),
            linkService.Create(nameof(UpdateHabit), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(PatchHabit), "patch", HttpMethods.Patch, new { id }),

            linkService.Create(
                nameof(HabitTagsController.UpsertHabitTags),
                "upsert-tags",
                HttpMethods.Put,
                new { habitId = id },
                HabitTagsController.Name)
        ];
        return links;
    }

    //方法重载：返回Page的Link
    private List<LinkDto> CreateLinksForHabits(
        HabitsQueryParameters parameters,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetHabits), "self", HttpMethods.Get, new
            {
                page = parameters.Page,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }),
            linkService.Create(nameof(CreateHabit), "create", HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "next-page", HttpMethods.Get, new
            {
                page = parameters.Page + 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }));
        }

        if (hasPreviousPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "previous-page", HttpMethods.Get, new
            {
                page = parameters.Page - 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status
            }));
        }

        return links;
    }

}
