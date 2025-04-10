using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace NCFDDClient.Utils
{
    internal static class Helper
    {
        public static List<string> GetNginxSitesEnabledDomains()
        {
            var sitesEnabledPath = "/etc/nginx/sites-enabled";

            if (Directory.Exists(sitesEnabledPath))
            {
                var files = Directory.GetFiles(sitesEnabledPath);
                if (files.Length > 0)
                {
                    var domainList = new HashSet<string>();
                    Regex serverNameRegex = new Regex(@"^\s*server_name\s+(.+?);", RegexOptions.IgnoreCase);

                    foreach (var file in files)
                    {
                        if (File.Exists(file))
                        {
                            string[] configLines = File.ReadAllLines(file);
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
