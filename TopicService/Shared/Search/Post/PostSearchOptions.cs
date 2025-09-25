using Shared.Filters;

namespace Shared.Search.Post;

public record PostSearchOptions(List<SortingEntry<PostSortOptions>>? SortEntries, PostFilter? Filter) 
    : SearchOptions<PostSortOptions, PostFilter>(SortEntries, Filter);
