namespace Shared.Filters;

public record TopicSearchParameters(
    string? SearchTerm,
    List<TopicFilterEntry> Filters);
