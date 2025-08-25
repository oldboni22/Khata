namespace Shared.PagedList;

public static class ListExtensions
{
    public static PagedList<T> ToPagedList<T>(this List<T> list, int pageNumber, int pageSize)
    {
        var pageCount = (int)Math.Ceiling((double)list.Count / pageSize);
        
        return new PagedList<T>(list, pageNumber, pageSize, pageCount, list.Count);
    }
}