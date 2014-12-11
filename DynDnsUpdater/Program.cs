using CookComputing.XmlRpc;
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

        static void Main(string[] args)
        {
            ParseArgs(args);
            string newIP = DynDnsUpdater.AddressFinder.GetIP();
            if (newIP == "")
            {
                Console.WriteLine("Unable to determine public IP.");
                Environment.Exit(1);
            }
            Console.WriteLine("Public IP: {0}", newIP);
            IDomainZone proxy = XmlRpcProxyGen.Create<IDomainZone>();
            ZoneListReturn[] list = proxy.ZoneList(_gandiApiKey);
            ZoneListReturn zoneResult = list.FirstOrDefault<ZoneListReturn>(zr => zr.name.Equals(_zoneName, StringComparison.InvariantCultureIgnoreCase));
            foreach (ZoneListReturn zl in list)
            {
                Console.WriteLine("Name: [{1}]{0}", zl.name, zl.id);
            }
            if (zoneResult.id == 0)
            {
                Console.WriteLine("Zone {0} not found.");
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("Found zone {0}. ID: {1}, Version: {2}", zoneResult.name, zoneResult.id, zoneResult.version);
                int newZoneVersion = proxy.ZoneNewVersion(_gandiApiKey, zoneResult.id);
                Console.WriteLine("Created zone version {0}.", newZoneVersion);
                ZoneRecordReturn[] records = proxy.RecordList(_gandiApiKey, zoneResult.id, newZoneVersion);
                ZoneRecordReturn record = records.FirstOrDefault<ZoneRecordReturn>(r => r.name.Equals(_hostName, StringComparison.InvariantCultureIgnoreCase));
                if (record.id == 0)
                {
                    // Add record
                    Console.WriteLine("Zone Record NOT Found. Adding...");
                    ZoneRecord newRecord;
                    newRecord.name = _hostName;
                    newRecord.type = "A";
                    newRecord.ttl = _defautlTTL;
                    newRecord.value = newIP;
                    ZoneRecordReturn result = proxy.RecordAdd(_gandiApiKey, zoneResult.id, newZoneVersion, newRecord);
                    Console.WriteLine("New Record ID: {0}", result.id);
                    if (result.id != 0)
                    {
                        // ZoneRecordReturn result = updateResults[0];
                        // Console.WriteLine("Record inserted. New record ID: {0}", result.id);
                        bool zoneUpdated = proxy.ZoneSetActiveVersion(_gandiApiKey, zoneResult.id, newZoneVersion);
                        if (zoneUpdated)
                        {
                            Console.WriteLine("Zone updated to version {0}.", newZoneVersion);
                        }
                        else
                        {
                            Console.WriteLine("Zone update failed.");
                            Environment.Exit(1);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unable to insert/add record. Deleting unused zone version {0}.", newZoneVersion);
                        proxy.ZoneDeleteVersion(_gandiApiKey, zoneResult.id, newZoneVersion);
                    }
                }
                else
                {
                    // Update record if changed
                    Console.WriteLine("Zone Record Found -");
                    Console.WriteLine("ID: {0}", record.id);
                    Console.WriteLine("Name: {0}", record.name);
                    Console.WriteLine("Type: {0}", record.type);
                    Console.WriteLine("Value: '{0}'", record.value);
                    Console.WriteLine("newIP: '{0}'", newIP);
                    Console.WriteLine("TTL: {0}", record.ttl);
                    if (record.value == newIP)
                    {
                        proxy.ZoneDeleteVersion(_gandiApiKey, zoneResult.id, newZoneVersion);
                        Console.WriteLine("IP Unchanged. Exiting.");
                        Environment.Exit(0);
                    }
                    if (record.type != "A")
                    { 
                        Console.WriteLine("Unable to update record. Not an A record.");
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
                    //Console.WriteLine("NumDel: {0}", numDel);
                    if (updateResults.Length > 0)
                    {
                        // ZoneRecordReturn result = updateResults[0];
                        // Console.WriteLine("Record inserted. New record ID: {0}", result.id);
                        bool zoneUpdated = proxy.ZoneSetActiveVersion(_gandiApiKey, zoneResult.id, newZoneVersion);
                        if (zoneUpdated)
                        {
                            Console.WriteLine("Zone updated to version {0}. Deleting previous...", newZoneVersion);
                            zoneUpdated = proxy.ZoneDeleteVersion(_gandiApiKey, zoneResult.id, zoneResult.version);
                        }
                        else
                        {
                            Console.WriteLine("Zone update failed.");
                            Environment.Exit(1);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unable to update record. Deleting unused zone version {0}.", newZoneVersion);
                        proxy.ZoneDeleteVersion(_gandiApiKey, zoneResult.id, newZoneVersion);
                    }
                }
            }
        }

        static void ParseArgs(string[] args)
        {
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
            Console.WriteLine("Usage: DynDnsUpdater.exe -apikey zzzzzzzzzzz -zonename example.com -hostname dynamichost");
            Environment.Exit(0);
        }

        static void ErrorAndExit(Exception e)
        {
            Console.WriteLine("Error Detected:");
            Console.Write(e.Message);
            Environment.Exit(1);
        }
    }
}
