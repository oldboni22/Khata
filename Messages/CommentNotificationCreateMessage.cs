namespace Messages;

public record class CommentNotificationCreateMessage(Guid UserId, Guid CommentId);
