namespace lgDevHabit.Api.DTOs.Common;

//返回List的接口
public interface ICollectionResponse<T>
{
    List<T> Items { get; init; }
}
