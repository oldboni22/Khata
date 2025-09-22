namespace NotificationService.API.Messages.Models;

public record class CommentNotificationCreateMessage(Guid UserId, Guid CommentId);
