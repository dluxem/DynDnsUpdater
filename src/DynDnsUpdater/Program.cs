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
        private static readonly int _defautlTTL = 10800;
        private static readonly string[] _defaulIpServices = { "http://icanhazip.com/", "http://ifconfig.me/ip" };

        static Program()
        {
            StaticLogger.LevelToOutput = StaticLogger.LogLevel.Debug;
            StaticLogger.MethodToLog = StaticLogger.LogMethod.Console;
        }
        static void Main(string[] args)
        {
            ParseArgs(args);
            AddressFinder finder = new AddressFinder(_defaulIpServices);
            string newIP = finder.GetIP();
            if (newIP == "")
            {
                StaticLogger.Log("Unable to determine public IP.");
                Environment.Exit(1);
            }
            StaticLogger.Log(StaticLogger.LogLevel.Info, String.Format("Public IP: {0}", newIP));
            IDomainZone proxy = XmlRpcProxyGen.Create<IDomainZone>();
            ZoneListReturn[] list = proxy.ZoneList(_gandiApiKey);
            ZoneListReturn zoneResult = list.FirstOrDefault<ZoneListReturn>(zr => zr.name.Equals(_zoneName, StringComparison.InvariantCultureIgnoreCase));
            foreach (ZoneListReturn zl in list)
            {
                StaticLogger.Log(String.Format("Name: [{1}]{0}", zl.name, zl.id));
            }
            if (zoneResult.id == 0)
            {
                StaticLogger.Log("Zone {0} not found.");
                Environment.Exit(1);
            }
            else
            {
                StaticLogger.Log(String.Format("Found zone {0}. ID: {1}, Version: {2}", zoneResult.name, zoneResult.id, zoneResult.version));
                int newZoneVersion = proxy.ZoneNewVersion(_gandiApiKey, zoneResult.id);
                StaticLogger.Log(String.Format("Created zone version {0}.", newZoneVersion));
                ZoneRecordReturn[] records = proxy.RecordList(_gandiApiKey, zoneResult.id, newZoneVersion);
                ZoneRecordReturn record = records.FirstOrDefault<ZoneRecordReturn>(r => r.name.Equals(_hostName, StringComparison.InvariantCultureIgnoreCase));
                if (record.id == 0)
                {
                    // Add record
                    StaticLogger.Log("Zone Record NOT Found. Adding...");
                    ZoneRecord newRecord;
                    newRecord.name = _hostName;
                    newRecord.type = "A";
                    newRecord.ttl = _defautlTTL;
                    newRecord.value = newIP;
                    ZoneRecordReturn result = proxy.RecordAdd(_gandiApiKey, zoneResult.id, newZoneVersion, newRecord);
                    StaticLogger.Log("New Record ID: {0}", result.id);
                    if (result.id != 0)
                    {
                        // ZoneRecordReturn result = updateResults[0];
                        // StaticLogger.Log("Record inserted. New record ID: {0}", result.id);
                        bool zoneUpdated = proxy.ZoneSetActiveVersion(_gandiApiKey, zoneResult.id, newZoneVersion);
                        if (zoneUpdated)
                        {
                            StaticLogger.Log("Zone updated to version {0}.", newZoneVersion);
                        }
                        else
                        {
                            StaticLogger.Log("Zone update failed.");
                            Environment.Exit(1);
                        }
                    }
                    else
                    {
                        StaticLogger.Log("Unable to insert/add record. Deleting unused zone version {0}.", newZoneVersion);
                        proxy.ZoneDeleteVersion(_gandiApiKey, zoneResult.id, newZoneVersion);
                    }
                }
                else
                {
                    // Update record if changed
                    StaticLogger.Log("Zone Record Found -");
                    StaticLogger.Log("ID: {0}", record.id);
                    StaticLogger.Log("Name: {0}", record.name);
                    StaticLogger.Log("Type: {0}", record.type);
                    StaticLogger.Log("Value: '{0}'", record.value);
                    StaticLogger.Log("newIP: '{0}'", newIP);
                    StaticLogger.Log("TTL: {0}", record.ttl);
                    if (record.value == newIP)
                    {
                        proxy.ZoneDeleteVersion(_gandiApiKey, zoneResult.id, newZoneVersion);
                        StaticLogger.Log("IP Unchanged. Exiting.");
                        Environment.Exit(0);
                    }
                    if (record.type != "A")
                    { 
                        StaticLogger.Log("Unable to update record. Not an A record.");
                        Environment.Exit(1);
                    }
                    ZoneRecord newRecord;
                    RecordUpdateOptions recordToUpdate;
                    recordToUpdate.id = record.id;
                    newRecord.name = record.name;
                    newRecord.type = "A";
                    newRecord.ttl = record.ttl;
                    newRecord.value = newIP;
                    ZoneRecordReturn[] updateResults = proxy.RecordUpdate(_gandiApiKey, zoneResult.id, newZoneVersion, recordToUpdate, newRecord);
                    //RecordDeleteOptions recordToDelete;
                    //recordToDelete.id = record.id;
                    //int numDel = proxy.RecordDelete(_gandiApiKey, test.id, newZoneVersion, recordToDelete);
                    //StaticLogger.Log("NumDel: {0}", numDel);
                    if (updateResults.Length > 0)
                    {
                        // ZoneRecordReturn result = updateResults[0];
                        // StaticLogger.Log("Record inserted. New record ID: {0}", result.id);
                        bool zoneUpdated = proxy.ZoneSetActiveVersion(_gandiApiKey, zoneResult.id, newZoneVersion);
                        if (zoneUpdated)
                        {
                            StaticLogger.Log("Zone updated to version {0}. Deleting previous...", newZoneVersion);
                            zoneUpdated = proxy.ZoneDeleteVersion(_gandiApiKey, zoneResult.id, zoneResult.version);
                        }
                        else
                        {
                            StaticLogger.Log("Zone update failed.");
                            Environment.Exit(1);
                        }
                    }
                    else
                    {
                        StaticLogger.Log("Unable to update record. Deleting unused zone version {0}.", newZoneVersion);
                        proxy.ZoneDeleteVersion(_gandiApiKey, zoneResult.id, newZoneVersion);
                    }
                }
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
