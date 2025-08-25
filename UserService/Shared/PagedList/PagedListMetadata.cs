namespace Shared.PagedList;

public record PagedListMetadata(int PageNumber, int PageSize, int PageCount, int TotalCount)
{
    public bool HasPrevious => PageNumber > 1;
    
    public bool HasNext => PageNumber < TotalCount;
}