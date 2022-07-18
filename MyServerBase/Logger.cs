using Cysharp.Text;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLogger;

namespace MyServerBase
{
    public static class Logger
    {
        static ILogger globalLogger;

        public static void Init(LogLevel logLevel)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(logLevel);
                builder.AddZLoggerConsole(options => 
                {
                    var prefixFormat = ZString.PrepareUtf8<LogLevel, DateTime>("[{0}][{1}]");
                    options.PrefixFormatter = (writer, info) => prefixFormat.FormatTo(ref writer, info.LogLevel, info.Timestamp.DateTime.ToLocalTime());
                });
            });

            globalLogger = loggerFactory.CreateLogger("Global");
        }

        public static void LogDebug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public static void LogInformation(string message)
        {
            Log(LogLevel.Information, message);
        }

        public static void LogWarning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public static void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }

        private static void Log(LogLevel logLevel, string messasge)
        {
            if (globalLogger == null)
            {
                Console.WriteLine("Need Logger.Init()");
                return;
            }

            switch(logLevel)
            {
                case LogLevel.Debug:
                    globalLogger.ZLogDebug(messasge);
                    break;
                case LogLevel.Information:
                    globalLogger.ZLogInformation(messasge);
                    break;
                case LogLevel.Warning:
                    globalLogger.ZLogWarning(messasge);
                    break;
                case LogLevel.Error:
                    globalLogger.ZLogError(messasge);
                    break;
                default:
                    Console.WriteLine($"Undefined LogLevel: {logLevel}");
                    break;
            }
        }
    }
}
