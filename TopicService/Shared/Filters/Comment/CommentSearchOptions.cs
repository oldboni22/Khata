namespace Shared.Filters.Comment;

public record CommentSearchOptions(
    string? SearchTerm, List<FilterEntry<CommentSortOptions>>? Filters
    ) : SearchOptions<CommentSortOptions>(SearchTerm, Filters);
