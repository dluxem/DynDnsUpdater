using CookComputing.XmlRpc;
using System;
using System.Linq;

namespace DynDnsUpdater.Gandi
{
    public class GandiService : IDnsService
    {
        private const string prodApiUrl = "https://rpc.gandi.net/xmlrpc/";
        private const string testApiUrl = "https://rpc.ote.gandi.net/xmlrpc/";

        private readonly string _zoneName;
        private readonly bool _simulate;
        private readonly string _apiKey;
        private IDomainZone _proxy = XmlRpcProxyGen.Create<IDomainZone>();

        private bool _ready = false;

        private ZoneListReturn _zone;

        public GandiService(bool useTestSystem, string apiKey, string zoneName, bool simulate)
        {
            StaticLogger.Log("Starting GandiService");
            _apiKey = apiKey;
            _zoneName = zoneName;
            _simulate = simulate;

            if (useTestSystem) { _proxy.Url = testApiUrl; }
            else { _proxy.Url = prodApiUrl; }
            StaticLogger.Log("Using proxy address " + _proxy.Url);

            StaticLogger.Log("Retrieving zone list...");
            try
            {
                ZoneListReturn[] list = _proxy.ZoneList(_apiKey);
                _zone = list.FirstOrDefault<ZoneListReturn>(zr => zr.name.Equals(zoneName, StringComparison.InvariantCultureIgnoreCase));
                foreach (ZoneListReturn zl in list)
                {
                    StaticLogger.Log(String.Format("Found Zone: [{1}]{0}", zl.name, zl.id));
                }
                _ready = true;
            }
            catch (Exception e)
            {
                StaticLogger.Log(StaticLogger.LogLevel.Error, "EXCEPTION: " + e.Message);
                _ready = false;
            }
            if (_zone.id == 0)
            {
                StaticLogger.Log("Zone {1} not found.", zoneName);
                _ready = false;
            }
        }

        public bool Ready()
        {
            return _ready;
        }

        public bool HostExists(string hostName)
        {
            ZoneRecordReturn record = GetHostRecord(hostName, _zone);
            if (record.id == 0)
            {
                StaticLogger.Log(StaticLogger.LogLevel.Info, String.Format("Host {0} NOT found in {1}. Zone ID {2}, Zone Version {3}", hostName, _zone.name, _zone.id, _zone.version));
                return false;
            }
            else
            {
                StaticLogger.Log(String.Format("Host {0} found in {1}. Zone ID {2}, Zone Version {3}", hostName, _zone.name, _zone.id, _zone.version));
                return true;
            }
        }

        public string GetHostIp(string hostName)
        {
            ZoneRecordReturn record = GetHostRecord(hostName, _zone);
            if (record.id == 0) { return null; }
            else
            {
                StaticLogger.Log("Host: {0} IP: " + record.value, hostName);
                return record.value;
            }
        }

        public bool UpdateHost(string hostName, string ipAddress, int defaultTTL)
        {
            if (!_ready) { return false; }
            int newZoneVersion = _proxy.ZoneNewVersion(_apiKey, _zone.id);
            StaticLogger.Log(String.Format("Created zone version {0}.", newZoneVersion));

            ZoneListReturn newZone = new ZoneListReturn();
            newZone.id = _zone.id;
            newZone.version = newZoneVersion;

            ZoneRecordReturn record = GetHostRecord(hostName, newZone);

            if (record.id == 0) { return false; }

            StaticLogger.Log("Zone Record Found -");
            StaticLogger.Log("ID: {0}", record.id);
            StaticLogger.Log("Name: {0}", record.name);
            StaticLogger.Log("Type: {0}", record.type);
            StaticLogger.Log("Value: '{0}'", record.value);
            StaticLogger.Log("newIP: '{0}'", ipAddress);
            StaticLogger.Log("TTL: {0}", record.ttl);
            if (record.type != "A")
            {
                StaticLogger.Log("Unable to update record. Not an A record.");
                return false;
            }
            ZoneRecord newRecord;
            RecordUpdateOptions recordToUpdate;
            recordToUpdate.id = record.id;
            newRecord.name = record.name;
            newRecord.type = "A";
            newRecord.ttl = defaultTTL;
            newRecord.value = ipAddress;
            ZoneRecordReturn[] updateResults = _proxy.RecordUpdate(_apiKey, newZone.id, newZoneVersion, recordToUpdate, newRecord);
            //RecordDeleteOptions recordToDelete;
            //recordToDelete.id = record.id;
            //int numDel = proxy.RecordDelete(_gandiApiKey, test.id, newZoneVersion, recordToDelete);
            //StaticLogger.Log("NumDel: {0}", numDel);
            if (updateResults.Length > 0)
            {
                // ZoneRecordReturn result = updateResults[0];
                // StaticLogger.Log("Record inserted. New record ID: {0}", result.id);
                bool zoneUpdated = _proxy.ZoneSetActiveVersion(_apiKey, newZone.id, newZoneVersion);
                if (zoneUpdated)
                {
                    StaticLogger.Log("Zone updated to version {0}. Deleting previous...", newZoneVersion);
                    zoneUpdated = _proxy.ZoneDeleteVersion(_apiKey, newZone.id, newZone.version);
                }
                else
                {
                    StaticLogger.Log("Zone update failed.");
                }
                return zoneUpdated;
            }
            else
            {
                StaticLogger.Log("Unable to update record. Deleting unused zone version {0}.", newZoneVersion);
                _proxy.ZoneDeleteVersion(_apiKey, newZone.id, newZoneVersion);
                return false;
            }
        }

        public bool AddHost(string hostName, string ipAddress, int defaultTTL)
        {
            if (!_simulate || !_ready)
            {
                int newZoneVersion = _proxy.ZoneNewVersion(_apiKey, _zone.id);
                StaticLogger.Log(String.Format("Created zone version {0}.", newZoneVersion));
                StaticLogger.Log("Zone Record NOT Found. Adding...");
                ZoneRecord newRecord;
                newRecord.name = hostName;
                newRecord.type = "A";
                newRecord.ttl = defaultTTL;
                newRecord.value = ipAddress;
                ZoneRecordReturn result = _proxy.RecordAdd(_apiKey, _zone.id, newZoneVersion, newRecord);
                StaticLogger.Log("New Record ID: {0}", result.id);
                if (result.id != 0)
                {
                    bool zoneUpdated = _proxy.ZoneSetActiveVersion(_apiKey, _zone.id, newZoneVersion);
                    if (zoneUpdated)
                    {
                        StaticLogger.Log(StaticLogger.LogLevel.Info, String.Format("HOST ADDED. HostName:{0} Type:{1} TTL:{2} Address:{3}", hostName, newRecord.type, newRecord.ttl, newRecord.value));
                        StaticLogger.Log("Zone updated to version {0}.", newZoneVersion);
                    }
                    else
                    {
                        StaticLogger.Log(StaticLogger.LogLevel.Error, "Zone update failed.");
                    }
                    return zoneUpdated;
                }
                else
                {
                    StaticLogger.Log("Unable to insert/add record. Deleting unused zone version {0}.", newZoneVersion);
                    _proxy.ZoneDeleteVersion(_apiKey, _zone.id, newZoneVersion);
                    return false;
                }
            }
            else
            {
                StaticLogger.Log(StaticLogger.LogLevel.Info, "Skipping AddHost (simulate mode/not ready)");
                return true;
            }

        }

        private ZoneRecordReturn GetHostRecord(string hostName, ZoneListReturn hostZone)
        {
            if (!_ready) { return new ZoneRecordReturn(); }
            ZoneRecordReturn[] records = _proxy.RecordList(_apiKey, hostZone.id, hostZone.version);
            return records.FirstOrDefault<ZoneRecordReturn>(r => r.name.Equals(hostName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
