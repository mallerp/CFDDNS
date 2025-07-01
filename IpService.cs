using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CFDDNS
{
    public static class IpService
    {
        private static readonly HttpClient HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private const string Ipv4ApiUrl = "http://v4.ipv6-test.com/api/myip.php";
        private const string Ipv6ApiUrl = "http://v6.ipv6-test.com/api/myip.php";

        public static async Task<string> GetPublicIpv4Async()
        {
            try
            {
                return await HttpClient.GetStringAsync(Ipv4ApiUrl);
            }
            catch (Exception)
            {
                return ""; // Return empty string on error
            }
        }

        public static async Task<string> GetPublicIpv6Async()
        {
            try
            {
                return await HttpClient.GetStringAsync(Ipv6ApiUrl);
            }
            catch (Exception)
            {
                // It's common for machines to not have IPv6 connectivity
                return ""; // Return empty string on error
            }
        }
    }
} 