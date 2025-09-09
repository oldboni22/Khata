namespace TopicService.API.Dto.Post;

public class PostReadDto
{
    public Guid Id { get; set; }
    
    public required string Text { get; set; }
    
    public required string Title { get; set; }
    
    public int LikeCount { get; set; }
    
    public int DislikeCount { get; set; }
}
