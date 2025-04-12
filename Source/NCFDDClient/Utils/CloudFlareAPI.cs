using NCFDDClient.Models;
using Newtonsoft.Json;
using System.Net;

namespace NCFDDClient.Utils
{
    internal static class CloudFlareAPI
    {
        private const string BaseURL = "https://api.cloudflare.com/client/v4";

        private static string _zoneID = string.Empty;
        private static string _apiToken = string.Empty;

        public static void SetZoneAndToken(string zoneID, string apiToken)
        {
            _zoneID = zoneID;
            _apiToken = apiToken;
        }

        public static bool VerifyApiToken()
        {
            using var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{BaseURL}/user/tokens/verify"),
                Headers = {
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {_apiToken}" },
                    { HttpRequestHeader.Accept.ToString(), "application/json" },
                }
            };

            try
            {
                using var response = Common.HttpClient.SendAsync(httpRequest).Result;

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    Logger.LogError($"[VerifyApiToken] Request failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[VerifyApiToken] Exception: {ex.Message}");
            }

            return false;
        }

        public static List<AAAARecord> GetDNSRecords(List<string> domainFilter)
        {
            var records = new List<AAAARecord>();

            using var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{BaseURL}/zones/{_zoneID}/dns_records?type=AAAA&per_page=5000"),
                Headers = {
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {_apiToken}" },
                    { HttpRequestHeader.Accept.ToString(), "application/json" },
                }
            };

            try
            {
                using var requestResult = Common.HttpClient.SendAsync(httpRequest).Result;

                if (requestResult.IsSuccessStatusCode)
                {
                    var response = JsonConvert.DeserializeObject<ApiResponse>(requestResult.Content.ReadAsStringAsync().Result);
                    if (response != null && response.Result != null)
                    {
                        foreach (var item in response.Result)
                        {
                            var domain = item.Name.Trim();
                            if (domainFilter.Contains(domain, StringComparer.OrdinalIgnoreCase))
                            {
                                records.Add(new AAAARecord
                                {
                                    ID = item.ID.Trim(),
                                    Name = domain,
                                    Content = item.Content.Trim(),
                                });
                            }
                        }
                    }
                    else
                    {
                        Logger.LogError("[GetDNSRecords] Failed to deserialize the response or result is null.");
                    }
                }
                else
                {
                    Logger.LogError($"[GetDNSRecords] Request failed with status code: {requestResult.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[GetDNSRecords] Exception: {ex.Message}");
            }

            return records;
        }

        public static bool UpdateDNSRecordsIP(List<AAAARecord> records, string ip)
        {
            var updateList = new List<Tuple<string, string>>();

            foreach (var record in records)
            {
                if (!record.Content.Equals(ip, StringComparison.OrdinalIgnoreCase))
                {
                    updateList.Add(new Tuple<string, string>(record.Name.ToLower(), $"{{\"id\":\"{record.ID}\",\"content\":\"{ip}\"}}"));
                }
            }

            if (updateList.Count == 0)
            {
                return true;
            }
            else
            {
                using var httpRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{BaseURL}/zones/{_zoneID}/dns_records/batch"),
                    Headers = {
                        { HttpRequestHeader.Authorization.ToString(), $"Bearer {_apiToken}" },
                        { HttpRequestHeader.Accept.ToString(), "application/json" },
                    },
                    Content = new StringContent($"{{\"patches\":[{string.Join(',', updateList.Select(i => i.Item2).ToList())}]}}")
                };

                try
                {
                    using var requestResult = Common.HttpClient.SendAsync(httpRequest).Result;

                    if (requestResult.IsSuccessStatusCode)
                    {
                        var domainList = updateList.Select(i => i.Item1).ToList();
                        domainList.Sort();
                        Logger.LogInfo($"CloudFlare updated domain(s): {string.Join(", ", domainList)}");
                        return true;
                    }
                    else
                    {
                        Logger.LogError($"[UpdateDNSRecordsIP] Request failed with status code: {requestResult.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[UpdateDNSRecordsIP] Exception: {ex.Message}");
                }
            }

            return false;
        }

        #region JSON object

        public class DNSRecord
        {
            public string ID { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }

        public class ApiResponse
        {
            public List<DNSRecord>? Result { get; set; }
        }

        #endregion
    }
}