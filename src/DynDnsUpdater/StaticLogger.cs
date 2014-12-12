using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynDnsUpdater
{
    public static class StaticLogger
    {
        private static LogMethod _logMethod;
        private static LogLevel _logLevel;
        private const LogLevel _defaultLevel = LogLevel.Debug;

        static StaticLogger()
        {
            _logMethod = LogMethod.Console;
            _logLevel = LogLevel.Info;
        }

        public static LogLevel LevelToOutput
        {
            get { return _logLevel; }
            set { _logLevel = value; }
        }

        public static LogMethod MethodToLog {
            get { return _logMethod;  }
            set { _logMethod = value; }
        }

        public static string LogFile { get; set; }

        public static void Log(LogLevel logLevel, string message)
        {
            if (logLevel <= _logLevel)
            {
                // Prepend Log Level
                string levelMessage;
                ConsoleColor messageColor;
                switch (logLevel)
                {
                    case LogLevel.Error:
                        messageColor = ConsoleColor.Red;
                        levelMessage = "ERROR:";
                        break;
                    case LogLevel.Info:
                        messageColor = ConsoleColor.White;
                        levelMessage = "INFO:";
                        break;
                    case LogLevel.Debug:
                        messageColor = ConsoleColor.DarkYellow;
                        levelMessage = "DEBUG:";
                        break;
                    default:
                        messageColor = ConsoleColor.DarkYellow;
                        levelMessage = "DEBUG:";
                        break;
                }
                
                if (_logMethod == LogMethod.Console || _logMethod == LogMethod.Tee)
                {
                    ConsoleColor defColor = Console.ForegroundColor;
                    Console.ForegroundColor = messageColor;
                    Console.Write(levelMessage + " ");
                    Console.ForegroundColor = defColor;
                    Console.WriteLine(message);
                }
                else if (_logMethod == LogMethod.File)
                {

                }


            }
        }

        public static void Log(string message)
        {
            Log(logLevel: _defaultLevel, message: message);
        }

        public static void Log(string messageToFormat, object formatObject)
        {
            Log(logLevel: _defaultLevel, message: String.Format(messageToFormat, formatObject));
        }

        public enum LogMethod
        {
            Console,
            File,
            Tee
        }

        public enum LogLevel
        {
            Error,
            Info,
            Debug
        }
    }


}
