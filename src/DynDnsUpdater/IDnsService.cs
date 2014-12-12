
namespace DynDnsUpdater
{
    interface IDnsService
    {
        bool Ready();
        bool HostExists(string hostName);
        string GetHostIp(string hostName);
        bool UpdateHost(string hostName, string ipAddress, int defaultTTL);
        bool AddHost(string hostName, string ipAddress, int defaultTTL);
    }
}
