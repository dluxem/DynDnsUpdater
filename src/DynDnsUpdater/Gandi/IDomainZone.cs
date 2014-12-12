using CookComputing.XmlRpc;

namespace DynDnsUpdater.Gandi
{
    [XmlRpcUrl("https://rpc.gandi.net/xmlrpc/")]
    interface IDomainZone
    {
        [XmlRpcMethod("domain.zone.list")]
        ZoneListReturn[] ZoneList(string apikey);

        [XmlRpcMethod("domain.zone.version.new")]
        int ZoneNewVersion(string apikey, int zone_id);

        [XmlRpcMethod("domain.zone.version.set")]
        bool ZoneSetActiveVersion(string apikey, int zone_id, int version_id);

        [XmlRpcMethod("domain.zone.version.delete")]
        bool ZoneDeleteVersion(string apikey, int zone_id, int version_id);

        [XmlRpcMethod("domain.zone.record.list")]
        ZoneRecordReturn[] RecordList(string apikey, int zone_id, int vesion_id);

        [XmlRpcMethod("domain.zone.record.add")]
        ZoneRecordReturn RecordAdd(string apikey, int zone_id, int version_id, ZoneRecord @params);

        [XmlRpcMethod("domain.zone.record.update")]
        ZoneRecordReturn[] RecordUpdate(string apikey, int zone_id, int version_id, RecordUpdateOptions opts, ZoneRecord @params);

        [XmlRpcMethod("domain.zone.record.delete")]
        int RecordDelete(string apikey, int zone_id, int version_id, RecordDeleteOptions opts);

    }

}
