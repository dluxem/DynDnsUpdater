﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DynDnsUpdater
{
    public static class Logger
    {
        private static LogMethod _logMethod;
        private static LogLevel _logLevel;
        private const LogLevel _defaultLevel = LogLevel.Debug;

        static Logger()
        {
            _logMethod = LogMethod.Console;
            _logLevel = LogLevel.Info;
            LogFile = "";
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
                if (_logMethod == LogMethod.File || _logMethod == LogMethod.Tee)
                {
                    try
                    {
                        if (LogFile.Length > 0)
                        {
                            File.AppendAllText(LogFile, DateTime.Now.ToString("s") + " " + levelMessage + " " + message + "\r\n");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error logging to file. " + e.Message);
                        LogFile = "";
                    }
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
