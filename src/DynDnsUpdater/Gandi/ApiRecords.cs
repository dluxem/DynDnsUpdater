using System;

namespace DynDnsUpdater.Gandi
{
    public struct ZoneListReturn
    {
        public DateTime date_updated;
        public int id;
        public string name;
        public bool @public;
        public int version;
    }

    public struct ZoneReturn
    {
        public DateTime date_updated;
        public int domains;
        public int id;
        public string name;
        public string owner;
        public bool @public;
        public int version;
        public int[] versions;
    }

    public struct ZoneRecordReturn
    {
        public int id;
        public string name;
        public string type;
        public string value;
        public int ttl;
    }

    public struct ZoneRecord
    {
        public string name;
        public string @type;
        public string value;
        public int ttl;
    }

    public struct RecordUpdateOptions
    {
        public int id;
    }

    public struct RecordDeleteOptions
    {
        public int id;
    }
}
