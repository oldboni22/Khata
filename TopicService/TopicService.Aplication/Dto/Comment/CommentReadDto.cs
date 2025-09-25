namespace TopicService.API.Dto.Comment;

public class CommentReadDto
{
    public Guid Id { get; set; }
    
    public Guid PostId { get; set; }
    
    public Guid UserId { get; set; }

    public required string Text { get; set; }
    
    public int LikeCount { get; set; }
    
    public int DislikeCount { get; set; }
}
