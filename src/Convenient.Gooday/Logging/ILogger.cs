using System;

namespace Convenient.Gooday.Logging
{
    public interface ILogger
    {
        void Log(object message);
    }

    public class ConsoleLogger : ILogger
    {
        public void Log(object message)
        {
            Console.WriteLine(message);
        }
    }
}