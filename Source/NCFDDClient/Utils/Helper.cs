using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace NCFDDClient.Utils
{
    internal static class Helper
    {
        private const string NginxPath = "/etc/nginx";

        public static List<string> GetSitesEnabledDomains()
        {
            var sitesEnabledPath = Path.Combine(NginxPath, "sites-enabled");
            var sitesAvailablePath = Path.Combine(NginxPath, "sites-available");

            if (Directory.Exists(sitesEnabledPath) && Directory.Exists(sitesAvailablePath))
            {
                var files = Directory.GetFiles(sitesEnabledPath).ToList();
                if (files.Count > 0)
                {
                    var domainList = new HashSet<string>();
                    files = files.Select(file => Path.GetFileName(file)).ToList();

                    Regex serverNameRegex = new Regex(@"^\s*server_name\s+(.+?);", RegexOptions.IgnoreCase);

                    foreach (var file in files)
                    {
                        var configFile = Path.Combine(sitesAvailablePath, file);
                        if (File.Exists(configFile))
                        {
                            string[] configLines = File.ReadAllLines(configFile);
                            foreach (var line in configLines)
                            {
                                if (line.Trim().StartsWith('#')) continue;

                                var match = serverNameRegex.Match(line);
                                if (match.Success)
                                {
                                    var domain = match.Groups[1].Value.Trim().ToLower();

                                    if (!domainList.Contains(domain))
                                    {
                                        domainList.Add(domain);
                                    }
                                }
                            }
                        }
                    }

                    if (domainList.Count > 0)
                    {
                        var sortedList = domainList.ToList();
                        sortedList.Sort();
                        return sortedList;
                    }
                }
            }

            return [];
        }

        public static string? GetPublicIPv6()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var netInterface in networkInterfaces)
            {
                var ipProperties = netInterface.GetIPProperties();
                var ipv6Address = ipProperties.UnicastAddresses.FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);

                if (ipv6Address != null && !ipv6Address.Address.IsIPv6LinkLocal)
                {
                    var ipv6 = ipv6Address.Address.ToString();
                    if (!ipv6.StartsWith(':'))
                    {
                        return ipv6;
                    }
                }
            }

            return null;
        }
    }
}
