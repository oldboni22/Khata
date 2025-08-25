namespace Shared.PagedList;

public static class ListExtensions
{
    public static PagedList<T> ToPagedList<T>(this List<T> list, int pageNumber, int pageSize)
    {
        return new PagedList<T>(list, pageNumber, pageSize);
    }
}