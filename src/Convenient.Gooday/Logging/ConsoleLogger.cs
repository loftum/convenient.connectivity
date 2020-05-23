using System;

namespace Convenient.Gooday.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Log(object message)
        {
            Console.WriteLine(message);
        }
    }
}