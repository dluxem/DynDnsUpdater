using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DynDnsUpdater
{
    public class AddressFinder
    {
        private static readonly string[] _ipServices = { "http://icanhazip.com/", "http://ifconfig.me/ip" };
        private static readonly WebClient _webCli = new WebClient();

        public static string GetIP()
        {
            string theIP = "";
            foreach (var ipService in _ipServices)
            {
                theIP = Try(ipService);
                if (theIP != "") { break; }
            }
            return theIP;
        }

        private static string Try(string url)
        {
            string results;
            Console.WriteLine("Checking {0}...", url);
            try
            {
                results = _webCli.DownloadString(url).Trim();
                Console.WriteLine("Found result: {0}", results);
            }
            catch (Exception e)
            {
                Console.Write("Error reaching URL: {0}", e.Message);
                results = "";
            }
            // Look for garbage
            // max length
            results = (results.Length <= 15) ? results : "";
            return results;
        }
    }
}
