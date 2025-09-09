namespace Shared.Filters.Post;

public record PostSearchParameters(
    string? SearchTerm,
    List<FilterEntry<PostSortOptions>> Filters);
