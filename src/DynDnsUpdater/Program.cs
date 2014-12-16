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
        private static ServiceType _selectedService = ServiceType.None;
        private static GandiSetup _gandiConfig;
        private static readonly int _defautlTTL = 3600;
        private static readonly string[] _defaulIpServices = { "http://icanhazip.com/", "http://ifconfig.me/ip" };

        static Program()
        {
            Logger.LevelToOutput = Logger.LogLevel.Info;
        }

        static void Main(string[] args)
        {
            ParseArgs(args);
            AddressFinder finder = new AddressFinder(_defaulIpServices);
            string newIP = finder.GetIP();
            if (newIP == "")
            {
                Logger.Log(Logger.LogLevel.Error, "Unable to determine public IP.");
                Environment.Exit(1);
            }
            Logger.Log(Logger.LogLevel.Info, String.Format("Public IP: {0}", newIP));

            IDnsService dnsSerivce = new GandiService(_gandiConfig);
            if (dnsSerivce.Ready()) {
                if (dnsSerivce.HostExists(_gandiConfig.HostName))
                {
                    // check if IP changed
                    if (dnsSerivce.GetHostIp(_gandiConfig.HostName) == newIP)
                    {
                        Logger.Log("IPs match. No update necessary.");
                    }
                    else {
                        Logger.Log(Logger.LogLevel.Info, "IP address changed. Updating to " + newIP);
                        dnsSerivce.UpdateHost(_gandiConfig.HostName, newIP, _defautlTTL); 
                    }
                }
                else
                {
                    Logger.Log(Logger.LogLevel.Info, "New host name. Adding host...");
                    dnsSerivce.AddHost(_gandiConfig.HostName, newIP, _defautlTTL);
                }
            }
            else
            {
                Logger.Log(Logger.LogLevel.Error, "Unable to initialize DnsService. Exiting...");
                Environment.Exit(1);
            }
        }

        static void ParseArgs(string[] args)
        {
            // for debugging from VS use the -stdin command line paramenter to read the regular parameters from the console
            if (args.Length == 1)
            {
                if (args[0] == "-stdin")
                {
                    Console.Write("Arguments: ");
                    args = Console.ReadLine().Split(' ');
                }
            }
            if (args.Length == 0) { PrintUsageAndExit(); }

            Dictionary<string,string> argList = new Dictionary<string,string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i][0] == '-')
                {
                    if (i == args.Length - 1 ) {
                        argList.Add(args[i].TrimStart('-').ToLower(), null); 
                    } else {
                        if (args[i+1][0] != '-') { argList.Add(args[i].TrimStart('-').ToLower(), args[1+1].Trim()); }
                        else { argList.Add(args[i].TrimStart('-').ToLower(), null); }
                    }
                }
            }

            // Parse the arguments by type
            // GANDI & GANDI TEST Environment
            if (argList.ContainsKey("ganditest") || argList.ContainsKey("gandi"))
            {
                // require apikey, zonename, hostname
                if (!argList.ContainsKey("apikey")
                    || !argList.ContainsKey("zonename")
                    || !argList.ContainsKey("hostname")
                    ) { PrintUsageAndExit(); }
                if (argList.ContainsKey("gandi"))
                {
                    _selectedService = ServiceType.Gandi;
                    _gandiConfig.UseTest = false;
                }
                else
                {
                    _selectedService = ServiceType.GandiTest;
                    _gandiConfig.UseTest = true;
                }
                _gandiConfig.ApiKey = argList["apikey"];
                _gandiConfig.HostName = argList["hostname"];
                _gandiConfig.ZoneName = argList["zonename"];
                _gandiConfig.Simulate = argList.ContainsKey("simulate");
            }

            if (argList.ContainsKey("debug")) { Logger.LevelToOutput = Logger.LogLevel.Debug; }
            if (argList.ContainsKey("logfile"))
            {
                Logger.LogFile = argList["logfile"];
                Logger.MethodToLog = Logger.LogMethod.Tee;
            }

            if (_selectedService == ServiceType.None) { PrintUsageAndExit(); }
        }

        static void PrintUsageAndExit()
        {
            string usage = String.Join(Environment.NewLine, new string[] { 
                "Usage Examples",
                "--------------",
                "Gandi:",
                "DynDnsUpdater.exe -gandi -apikey zzzzzzzzzzz -zonename example.com -hostname dynamichost [-simulate] [-debug] [-logfile filename]",
                " ",
                "Gandi OT&E (Test):",
                "DynDnsUpdater.exe -ganditest -apikey zzzzzzzzzzz -zonename example.com -hostname dynamichost [-simulate] [-debug] [-logfile filename]"
            });
            Console.WriteLine(usage);
            Environment.Exit(5);
        }

        static void ErrorAndExit(Exception e)
        {
            Logger.Log("Error Detected:");
            Console.Write(e.Message);
            Environment.Exit(1);
        }

        enum ServiceType
        {
            None,
            Gandi,
            GandiTest
        }
    }
}
