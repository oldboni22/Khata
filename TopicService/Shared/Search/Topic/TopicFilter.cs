namespace Shared.Search.Topic;

public record TopicFilter(string? SearchTerm) : Filter(SearchTerm);
