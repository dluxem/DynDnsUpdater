using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynDnsUpdater.Gandi
{
    public class GandiSetup
    {
        public string ZoneName { get; set; }
        public string HostName { get; set; }
        public string ApiKey { get; set; }
        public bool Simulate { get; set; }
        public bool UseTest { get; set; }
    }
}
