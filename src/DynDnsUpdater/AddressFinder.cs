using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DynDnsUpdater
{
    public class AddressFinder
    {
        private string[] _ipServices;
        private readonly WebClient _webCli = new WebClient();

        public AddressFinder(string[] ipServicesList)
        {
            _ipServices = ipServicesList;
        }

        public string GetIP()
        {
            string theIP = "";
            foreach (var ipService in _ipServices)
            {
                theIP = Try(ipService);
                if (theIP != "") { break; }
            }
            return theIP;
        }

        private string Try(string url)
        {
            string results;
            StaticLogger.Log("Checking {0}...", url);
            try
            {
                results = _webCli.DownloadString(url).Trim();
                StaticLogger.Log("Found result: {0}", results);
            }
            catch (Exception e)
            {
                StaticLogger.Log(StaticLogger.LogLevel.Error, "Error reaching URL: " + e.Message);
                results = "";
            }
            // Look for garbage
            // max length
            results = (results.Length <= 15) ? results : "";
            return results;
        }
    }
}
