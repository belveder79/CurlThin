using System.Diagnostics;
#if !UNITY
using Microsoft.Extensions.Logging;
#endif
namespace CurlThin
{
    public static class Logging
    {
#if !UNITY
        public static ILoggerFactory Factory { get; } = new LoggerFactory();

        internal static ILogger GetCurrentClassLogger()
        {
            return Factory.CreateLogger(
                new StackFrame(1).GetMethod().DeclaringType
            );
        }
#else
        private static readonly Logger instance = new Logger();

        public interface ILogger
        {
            void LogDebug(string msg);
            void LogTrace(string msg);
        }

        public class Logger : ILogger
        {
            static Logger()
            {

            }
            ~Logger() { }

            public void LogDebug(string msg)
            {
                System.Console.WriteLine("DEBUG: " + msg);
            }

            public void LogTrace(string msg)
            {
                System.Console.WriteLine("TRACE: " + msg);
            }
        }

        internal static ILogger GetCurrentClassLogger()
        {
            return instance;
        }
#endif
    }
}