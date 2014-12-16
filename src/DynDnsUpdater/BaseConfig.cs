using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynDnsUpdater
{
    public abstract class BaseConfig
    {
        public string ZoneName { get; set; }
        public string HostName { get; set; }
        public bool Simulate { get; set; }
    }
}
