using Shared.Search;

namespace Shared.Filters;

public abstract record SearchOptions<TSortOptions, TFilter>(List<SortingEntry<TSortOptions>>? SortEntries, TFilter? Filter)
    where TSortOptions : Enum
    where TFilter : Filter;
