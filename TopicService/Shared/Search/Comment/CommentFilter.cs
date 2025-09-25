namespace Shared.Search.Comment;

public record CommentFilter(string? SearchTerm) : Filter(SearchTerm);
