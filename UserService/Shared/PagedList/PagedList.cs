namespace Shared.PagedList;

public class PagedList<T>
{
    public List<T> Items { get; } = [];
    public PagedListMetadata Metadata { get; }
    
    internal PagedList(List<T> items, int pageSize, int pageNumber)
    {
        Metadata = new PagedListMetadata
        (
            PageNumber:  pageNumber,
            PageSize:  pageSize,
            PageCount:  (int)Math.Ceiling(items.Count / (double)pageSize),
            TotalCount:  items.Count
        );
        
        Items.AddRange(items);
    }
}