using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynDnsUpdater
{
    interface IDnsService
    {
        bool HostExists(string hostname);
        string GetHostIp(string hostname);
        bool UpdateHost(string hostname, string ipAddress);
        bool AddHost(string hostname, string ipAddress);
    }
}
