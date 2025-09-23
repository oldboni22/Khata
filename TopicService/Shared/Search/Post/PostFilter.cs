namespace Shared.Search.Post;

public record PostFilter(string? SearchTerm, Guid? UserId, int MinLikes = 0) : Filter(SearchTerm);
