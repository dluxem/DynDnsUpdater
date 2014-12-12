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
        private static string _gandiApiKey;
        private static string _zoneName;
        private static string _hostName;
        private static readonly int _defautlTTL = 3600;
        private static readonly string[] _defaulIpServices = { "http://icanhazip.com/", "http://ifconfig.me/ip" };

        static Program()
        {
            StaticLogger.LevelToOutput = StaticLogger.LogLevel.Debug;
            StaticLogger.LogFile = "DynDnsUpdater.log";
            StaticLogger.MethodToLog = StaticLogger.LogMethod.Tee;
        }

        static void Main(string[] args)
        {
            ParseArgs(args);
            AddressFinder finder = new AddressFinder(_defaulIpServices);
            string newIP = finder.GetIP();
            if (newIP == "")
            {
                StaticLogger.Log(StaticLogger.LogLevel.Error, "Unable to determine public IP.");
                Environment.Exit(1);
            }
            StaticLogger.Log(StaticLogger.LogLevel.Info, String.Format("Public IP: {0}", newIP));

            IDnsService dnsSerivce = new GandiService(false, _gandiApiKey, _zoneName, true);
            if (dnsSerivce.Ready()) {
                if (dnsSerivce.HostExists(_hostName))
                {
                    // check if IP changed
                    if (dnsSerivce.GetHostIp(_hostName) == newIP)
                    {
                        StaticLogger.Log("IPs match. No update necessary.");
                    }
                    else {
                        StaticLogger.Log(StaticLogger.LogLevel.Info, "IP address changed. Updating to " + newIP);
                        dnsSerivce.UpdateHost(_hostName, newIP, _defautlTTL); 
                    }
                }
                else
                {
                    StaticLogger.Log(StaticLogger.LogLevel.Info, "New host name. Adding host...");
                    dnsSerivce.AddHost(_hostName, newIP, _defautlTTL);
                }
            }
            else
            {
                StaticLogger.Log(StaticLogger.LogLevel.Error, "Unable to initialize DnsService. Exiting...");
                Environment.Exit(1);
            }
        }

        static void ParseArgs(string[] args)
        {
            // for debugging from VS use the -stdin command line paramenter to read the regular parameters from the console
            if (args[0] == "-stdin")
            {
                Console.Write("Arguments: ");
                args = Console.ReadLine().Split(' ');
            }

            if (args.Length != 6)
            {
                PrintUsageAndExit();
            }
            Dictionary<string,string> argList = new Dictionary<string,string>();
            argList.Add(args[0].TrimStart('-').ToLower(), args[1].Trim());
            argList.Add(args[2].TrimStart('-').ToLower(), args[3].Trim());
            argList.Add(args[4].TrimStart('-').ToLower(), args[5].Trim());
            if (argList["apikey"] == null) { PrintUsageAndExit();  }
            else { _gandiApiKey = argList["apikey"]; }
            if (argList["zonename"] == null) { PrintUsageAndExit(); }
            else { _zoneName = argList["zonename"]; }
            if (argList["hostname"] == null) { PrintUsageAndExit(); }
            else { _hostName = argList["hostname"]; }
        }

        static void PrintUsageAndExit()
        {
            StaticLogger.Log("Usage: DynDnsUpdater.exe -apikey zzzzzzzzzzz -zonename example.com -hostname dynamichost");
            Environment.Exit(0);
        }

        static void ErrorAndExit(Exception e)
        {
            StaticLogger.Log("Error Detected:");
            Console.Write(e.Message);
            Environment.Exit(1);
        }
    }
}
