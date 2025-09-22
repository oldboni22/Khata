namespace Shared.Filters.Post;

public record PostSearchOptions(
    string? SearchTerm, List<FilterEntry<PostSortOptions>>? Filters
) : SearchOptions<PostSortOptions>(SearchTerm, Filters);
