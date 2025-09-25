using Shared.Filters;

namespace Shared.Search.Comment;

public record CommentSearchOptions(List<SortingEntry<CommentSortOptions>>? SortEntries, CommentFilter? Filter)
    : SearchOptions<CommentSortOptions, CommentFilter>(SortEntries, Filter);
