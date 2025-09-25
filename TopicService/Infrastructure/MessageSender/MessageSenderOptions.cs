namespace Infrastructure.MessageSender;

public class MessageSenderOptions
{
    public const string SectionName = "MessageSender";
    
    public string QueueName { get; set; }
}
