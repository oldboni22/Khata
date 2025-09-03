namespace Shared.Filters;

public record TopicSearchFilter(
    string? SearchTerm,
    TopicSortOptions SortOptions = TopicSortOptions.None,
    bool Ascending = false);
