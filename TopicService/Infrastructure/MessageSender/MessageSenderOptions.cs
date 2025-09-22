namespace Infrastructure.MessageSender;

public class MessageSenderOptions
{
    public const string SectionName = "MessageSender";
    
    public string PostQueueName { get; set; }
    
    public string CommentQueueName { get; set; }
}
