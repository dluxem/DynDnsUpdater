using DynDnsUpdater.Gandi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynDnsUpdater
{
    public class DynDnsUpdater
    {
        private ServiceType _selectedService = ServiceType.None;
        private GandiConfig _gandiConfig = new GandiConfig();
        private readonly int _defautlTTL = 3600;
        private readonly string[] _defaulIpServices = { "http://icanhazip.com/", "http://ifconfig.me/ip" };

        public DynDnsUpdater(Dictionary<string,string> args)
        {
            // Parse the arguments by type
            // GANDI & GANDI TEST Environment
            if (args.ContainsKey("ganditest") || args.ContainsKey("gandi")) 
            {
                // require apikey, zonename, hostname
                if (args.ContainsKey("apikey")
                    && args.ContainsKey("zonename")
                    && args.ContainsKey("hostname")
                    )
                {
                    if (args.ContainsKey("gandi"))
                    {
                        _selectedService = ServiceType.Gandi;
                        _gandiConfig.UseTest = false;
                    }
                    else
                    {
                        _selectedService = ServiceType.GandiTest;
                        _gandiConfig.UseTest = true;
                    }
                    _gandiConfig.ApiKey = args["apikey"];
                    _gandiConfig.HostName = args["hostname"];
                    _gandiConfig.ZoneName = args["zonename"];
                    _gandiConfig.Simulate = args.ContainsKey("simulate");
                }
            }
        }

        public bool Execute() {
            if (_selectedService == ServiceType.None) { return false; }

            AddressFinder finder = new AddressFinder(_defaulIpServices);
            string newIP = finder.GetIP();
            if (newIP == "")
            {
                Logger.Log(Logger.LogLevel.Error, "Unable to determine public IP.");
                Environment.Exit(1);
            }
            Logger.Log(String.Format("Public IP: {0}", newIP));

            IDnsService dnsService;
            BaseConfig config;
            switch (_selectedService)
            {
                case ServiceType.None:
                    goto default;
                case ServiceType.Gandi:
                    dnsService = new GandiService(_gandiConfig);
                    config = _gandiConfig;
                    break;
                case ServiceType.GandiTest:
                    dnsService = new GandiService(_gandiConfig);
                    config = _gandiConfig;
                    break;
                default:
                    return false;
            }

            if (dnsService.Ready())
            {
                if (dnsService.HostExists(config.HostName))
                {
                    // check if IP changed
                    if (dnsService.GetHostIp(config.HostName) == newIP)
                    {
                        Logger.Log("IPs match. No update necessary.");
                    }
                    else
                    {
                        Logger.Log(Logger.LogLevel.Info, "IP address changed. Updating to " + newIP);
                        dnsService.UpdateHost(config.HostName, newIP, _defautlTTL);
                    }
                }
                else
                {
                    Logger.Log(Logger.LogLevel.Info, "New host name. Adding host...");
                    dnsService.AddHost(config.HostName, newIP, _defautlTTL);
                }
            }
            else
            {
                Logger.Log(Logger.LogLevel.Error, "Unable to initialize DnsService. Exiting...");
            }
            return true;
        }
        
        enum ServiceType
        {
            None,
            Gandi,
            GandiTest
        }

    }
}
