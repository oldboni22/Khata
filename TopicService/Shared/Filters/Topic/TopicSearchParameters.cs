namespace Shared.Filters.Topic;

public record TopicSearchParameters(
    string? SearchTerm,
    List<TopicFilterEntry> Filters);
