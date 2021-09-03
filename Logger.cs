using System;

namespace AAARunCheck
{
    public static class Logger
    {
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warn = 2,
            Error = 3
        }

        public static LogLevel CurrentLogLevel;

        public static void LogDebug(string message, params object[] args)
        {
            if (CurrentLogLevel <= LogLevel.Debug)
                Console.WriteLine("[DEBUG] " + message, args);
        }

        public static void LogInfo(string message, params object[] args)
        {
            if (CurrentLogLevel <= LogLevel.Info)
                Console.WriteLine("[INFO]  " + message, args);
        }

        public static void LogWarn(string message, params object[] args)
        {
            if (CurrentLogLevel <= LogLevel.Warn)
                Console.WriteLine("[WARN]  " + message, args);
        }

        public static void LogError(string message, params object[] args)
        {
            if (CurrentLogLevel <= LogLevel.Error)
                Console.WriteLine("[ERROR] " + message, args);
        }
    }
}