using System.Net.NetworkInformation;

namespace NCFDDClient.Utils
{
    internal static class Network
    {
        private static List<string> _ipv6List = new();
        private static string _mainIPv6 = string.Empty;

        public static string? GetPublicIPv6()
        {
            var ipv6List = GetIPv6List();
            if (ipv6List.Count == 0)
            {
                return null;
            }

            if (_ipv6List.Count == 0 || !ipv6List.SequenceEqual(_ipv6List))
            {
                _ipv6List = ipv6List;

                var ipifyIP = GetIpifyIPv6();
                if (ipifyIP != null && _ipv6List.Contains(ipifyIP))
                {
                    _mainIPv6 = ipifyIP;
                }
                else
                {
                    _mainIPv6 = _ipv6List[0];
                }
            }

            return _mainIPv6;
        }

        #region Private Functions

        private static string _interfaceName = string.Empty;

        private static List<string> GetIPv6List()
        {
            var networkInterfaces = string.IsNullOrEmpty(_interfaceName) ? NetworkInterface.GetAllNetworkInterfaces()
                : NetworkInterface.GetAllNetworkInterfaces().Where(i => i.Name.Equals(_interfaceName, StringComparison.OrdinalIgnoreCase));

            var ipv6List = new List<string>();

            foreach (var netInterface in networkInterfaces)
            {
                var ipv6Addresses = netInterface.GetIPProperties().UnicastAddresses
                    .Where(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && !ip.Address.IsIPv6LinkLocal && !ip.Address.IsIPv6UniqueLocal)
                    .Select(ip => ip.Address.ToString()).Where(ip => !ip.StartsWith(':')).ToList();

                foreach (var ipv6Address in ipv6Addresses)
                {
                    ipv6List.Add(ipv6Address.Trim().ToLower());
                }

                if (ipv6List.Count > 0)
                {
                    if (string.IsNullOrEmpty(_interfaceName))
                    {
                        _interfaceName = netInterface.Name.Trim();
                    }

                    ipv6List.Sort();
                    return ipv6List;
                }
            }

            return [];
        }

        private static string? GetIpifyIPv6()
        {
            const string apiUrl = "https://api6.ipify.org/";

            try
            {
                using var response = Common.HttpClient.GetAsync(apiUrl).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result.Trim().ToLower();
            }
            catch (Exception ex)
            {
                Logger.LogError($"[GetIpifyIPv6] Exception: {ex.Message}");
            }

            return null;
        }

        #endregion
    }
}