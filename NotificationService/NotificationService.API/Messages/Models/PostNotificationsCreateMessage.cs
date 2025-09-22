namespace NotificationService.API.Messages.Models;

public record PostNotificationsCreateMessage(Guid PostId, IEnumerable<Guid> UsersIds);
