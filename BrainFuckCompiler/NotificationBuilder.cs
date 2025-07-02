namespace BrainFuckCompiler;
internal static class NotificationBuilder {
    public enum NotificationType {
        Info, Warn, Exc, Suc, Other
    }
    public static string BuildNotification(string message, NotificationType type = NotificationType.Other) {
        string sign;
        switch (type) {
            case NotificationType.Info:
                sign = ".";
                break;
            case NotificationType.Warn:
                sign = "!";
                break;
            case NotificationType.Exc:
                sign = "-";
                break;
            case NotificationType.Suc:
                sign = "+";
                break;
            default:
                sign = " ";
                break;
        }

        return $"[ {sign} ] {message}";
    }
}
