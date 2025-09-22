namespace Messages;

public record PostNotificationsCreateMessage(Guid PostId, IEnumerable<Guid> UsersIds);
