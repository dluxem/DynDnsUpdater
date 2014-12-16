using CookComputing.XmlRpc;
using DynDnsUpdater.Gandi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DynDnsUpdater
{
    class Program
    {
        static Program()
        {
            Logger.LevelToOutput = Logger.LogLevel.Info;
        }

        static void Main(string[] args)
        {
            var parsedArgs = ParseArgs(args);
            var update = new DynDnsUpdater(parsedArgs);
            if (!update.Execute()) { PrintUsageAndExit();  }
        }

        static Dictionary<string,string> ParseArgs(string[] args)
        {
            // for debugging from VS use the -stdin command line paramenter to read the regular parameters from the console
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "-stdin":
                        Console.Write("Arguments: ");
                        args = Console.ReadLine().Split(' ');
                        break;
                    case "-?":
                        goto case "/?";
                    case "/?":
                        PrintUsageAndExit();
                        break;
                    default:
                        break;
                }
            }
            else { PrintUsageAndExit(); }

            Dictionary<string,string> argList = new Dictionary<string,string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (!String.IsNullOrEmpty(args[i]))
                {
                    if (args[i][0] == '-')
                    {
                        if (i == args.Length - 1)
                        {
                            argList.Add(args[i].TrimStart('-').ToLower(), null);
                        }
                        else
                        {
                            if (args[i + 1][0] != '-') { argList.Add(args[i].TrimStart('-').ToLower(), args[i + 1].Trim()); }
                            else { argList.Add(args[i].TrimStart('-').ToLower(), null); }
                        }
                    }
                }
            }

            if (argList.ContainsKey("debug")) { Logger.LevelToOutput = Logger.LogLevel.Debug; }
            if (argList.ContainsKey("logfile"))
            {
                Logger.LogFile = argList["logfile"];
                Logger.MethodToLog = Logger.LogMethod.Tee;
            }

            return argList;
        }

        static void PrintUsageAndExit()
        {
            string usage = String.Join(Environment.NewLine, new string[] {
                " ",
                "USAGE EXAMPLES",
                "==============",
                " ",
                "Common Parameters:",
                "[-debug]            Turns on debug logging",
                "[-logfile filename] Log output to filename",
                "[-simulate]         Test and avoid permenant changes",
                " ",
                "Gandi:",
                "DynDnsUpdater.exe -gandi -apikey zzzzzzzzzzz -zonename example.com -hostname dynamichost",
                " ",
                "Gandi OT&E (Test):",
                "DynDnsUpdater.exe -ganditest -apikey zzzzzzzzzzz -zonename example.com -hostname dynamichost"
            });
            Console.WriteLine(usage);
            Environment.Exit(5);
        }
    }
}
