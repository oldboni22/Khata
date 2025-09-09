namespace Shared.Filters.Comment;

public record CommentSearchParameters(
    string? SearchTerm,
    List<FilterEntry<CommentSortOptions>> Filters);
