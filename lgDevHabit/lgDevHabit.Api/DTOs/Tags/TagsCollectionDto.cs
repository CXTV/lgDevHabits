using lgDevHabit.Api.DTOs.Common;

namespace lgDevHabit.Api.DTOs.Tags;

public sealed record TagsCollectionDto: ICollectionResponse<TagDto>, ILinksResponse
{
    public List<TagDto> Items { get; init; }
    public List<LinkDto> Links { get; set; }

}
