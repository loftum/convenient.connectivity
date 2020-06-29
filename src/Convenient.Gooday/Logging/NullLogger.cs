namespace Convenient.Gooday.Logging
{
    public class NullLogger : ILogger
    {
        public void Write(LogImportance importance, LogMessageType type, object message)
        {
        }
    }
}