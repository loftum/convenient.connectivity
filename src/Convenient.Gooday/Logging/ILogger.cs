namespace Convenient.Gooday.Logging
{
    public interface ILogger
    {
        void Write(LogImportance importance, LogMessageType type, object message);
    }

    public static class LoggerExtensions
    {
        public static void Trace(this ILogger logger, object message) => logger?.Write(LogImportance.Trace, LogMessageType.Debug, message);
        public static void Debug(this ILogger logger, object message) => logger?.Write(LogImportance.Debug, LogMessageType.Debug, message);
        public static void Info(this ILogger logger, object message) => logger?.Write(LogImportance.Normal, LogMessageType.Info, message);
        public static void Warn(this ILogger logger, object message) => logger?.Write(LogImportance.Normal, LogMessageType.Warning, message);
        public static void Error(this ILogger logger, object message) => logger?.Write(LogImportance.Important, LogMessageType.Error, message);
    }
}