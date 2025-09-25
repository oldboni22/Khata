using Shared.Filters;

namespace Shared.Search.Topic;

public record TopicSearchParameters(List<SortingEntry<TopicSortOptions>>? SortEntries ,TopicFilter? Filter)
    : SearchOptions<TopicSortOptions, TopicFilter>(SortEntries, Filter);
