namespace Shared.PagedList;

public record PagedList<T>(List<T> Items, int PageNumber, int PageSize, int PageCount, int TotalCount)
{
    public bool HasPrevious => PageNumber > 1;
    
    public bool HasNext => PageNumber < PageCount;
}
