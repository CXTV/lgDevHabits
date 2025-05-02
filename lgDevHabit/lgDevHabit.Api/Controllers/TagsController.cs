using FluentValidation;
using FluentValidation.Results;
using lgDevHabit.Api.Database;
using lgDevHabit.Api.DTOs.Common;
using lgDevHabit.Api.DTOs.Tags;
using lgDevHabit.Api.Entities;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lgDevHabit.Api.Services;
using Microsoft.Extensions.Options;
using lgDevHabit.Api.Settings;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;

namespace lgDevHabit.Api.Controllers;

[ResponseCache(Duration = 120)]
[ApiController]
[Authorize(Roles = Roles.Member)]
[Route("tags")]
[Produces(
    MediaTypeNames.Application.Json,
    CustomMediaTypeNames.Application.JsonV1,
    CustomMediaTypeNames.Application.HateoasJson,
    CustomMediaTypeNames.Application.HateoasJsonV1)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public sealed class TagsController(
    ApplicationDbContext dbContext,
    LinkService linkService,
    UserContext userContext,
    IOptions<TagsOptions> options

    ) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<TagsCollectionDto>> GetTags()
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }


        List<TagDto> tags = await dbContext
            .Tags
            .Where(t => t.UserId == userId)

            .Select(TagQueries.ProjectToDto())
            .ToListAsync();

        var tagsCollectionDto = new TagsCollectionDto
        {
            Items = tags
        };




        return Ok(tagsCollectionDto);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(string id, [FromHeader] AcceptHeaderDto acceptHeader)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        TagDto? tag = await dbContext
            .Tags
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(TagQueries.ProjectToDto())
            .FirstOrDefaultAsync();

        if (tag is null)
        {
            return NotFound();
        }

        if (acceptHeader.IncludeLinks)
        {
            tag.Links = CreateLinksForTags();
        }
        return Ok(tag);
    }


    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(
        CreateTagDto createTagDto,
        [FromHeader] AcceptHeaderDto acceptHeader,
        IValidator<CreateTagDto> validator,
        ProblemDetailsFactory problemDetailsFactory
    )
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        ValidationResult validationResult = await validator.ValidateAsync(createTagDto);

        if (!validationResult.IsValid)
        {
            ProblemDetails problem = problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                StatusCodes.Status400BadRequest);
            problem.Extensions.Add("errors", validationResult.ToDictionary());

            return BadRequest(problem);
        }

        //判断是否超过最大标签数
        if (await dbContext.Tags.CountAsync(t => t.UserId == userId) >= options.Value.MaxAllowedTags)
        {

            return Problem(
                "Reached the maximum number of allowed tags",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Tag tag = createTagDto.ToEntity(userId);

        if (await dbContext.Tags.AnyAsync(t => t.Name == tag.Name))
        {
            return Conflict($"The tag '{tag.Name}' already exists");
        }

        dbContext.Tags.Add(tag);

        await dbContext.SaveChangesAsync();

        TagDto tagDto = tag.ToDto();

        if (acceptHeader.IncludeLinks)
        {
            tagDto.Links = CreateLinksForTag(tagDto.Id);
        }

        return CreatedAtAction(nameof(GetTag), new { id = tagDto.Id }, tagDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(string id, UpdateTagDto updateTagDto, InMemoryETagStore etagStore)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (tag is null)
        {
            return NotFound();
        }

        tag.UpdateFromDto(updateTagDto);
        await dbContext.SaveChangesAsync();

        etagStore.SetETag(Request.Path.Value!, tag.ToDto());


        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (tag is null)
        {
            return NotFound();
        }

        dbContext.Tags.Remove(tag);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private List<LinkDto> CreateLinksForTags()
    {
        return
        [
            linkService.Create(nameof(GetTags), "self", HttpMethods.Get),
            linkService.Create(nameof(CreateTag), "create", HttpMethods.Post)
        ];
    }

    private List<LinkDto> CreateLinksForTag(string id)
    {
        return
        [
            linkService.Create(nameof(GetTag), "self", HttpMethods.Get, new { id }),
            linkService.Create(nameof(UpdateTag), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(DeleteTag), "delete", HttpMethods.Delete, new { id })
        ];
    }
}
