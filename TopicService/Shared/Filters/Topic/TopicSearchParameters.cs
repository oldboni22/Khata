namespace Shared.Filters.Topic;

public record TopicSearchParameters(
    string? SearchTerm, List<FilterEntry<TopicSortOptions>>? Filters
) : SearchOptions<TopicSortOptions>(SearchTerm, Filters);
