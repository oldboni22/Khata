using Messages.Models;

namespace Messages;

public record NotificationsCreateMessage(IEnumerable<Notification> Notifications);