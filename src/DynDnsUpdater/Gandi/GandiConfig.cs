using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynDnsUpdater.Gandi
{
    public class GandiConfig : BaseConfig
    {
        public string ApiKey { get; set; }
        public bool UseTest { get; set; }
    }
}
