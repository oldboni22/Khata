using Shared.PagedList;

namespace Shared.Extensions;

public static class ListExtensions
{
    public static PagedList<T> ToPagedList<T>(this List<T> list, int pageNumber, int pageSize, int pageCount, int totalCount)
    {
        return new PagedList<T>(list, pageNumber, pageSize, pageCount, totalCount);
    }
}
