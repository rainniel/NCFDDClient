﻿using NCFDDClient.Utils;
using DotNetEnv;

#region Read .env

Env.Load();
string cfZoneID = Env.GetString("cf_zone_id", string.Empty);
string cfApiToken = Env.GetString("cf_api_token", string.Empty);

if (string.IsNullOrWhiteSpace(cfZoneID) || string.IsNullOrWhiteSpace(cfApiToken))
{
    if (string.IsNullOrEmpty(cfZoneID))
    {
        Logger.LogError("The environment variable 'cf_zone_id' is missing. Cloudflare Zone ID is required.");
    }

    if (string.IsNullOrEmpty(cfApiToken))
    {
        Logger.LogError("The environment variable 'cf_api_token' is missing. Cloudflare API token is required.");
    }

    Logger.LogError("Cannot continue without 'cf_zone_id' and/or 'cf_api_token'. Service terminated.");
    Environment.Exit(1);
}

int cfRequestInterval = Env.GetInt("cf_request_interval", 20);
int ipCheckInterval = Env.GetInt("ip_check_interval", 10);

// Set minimum value
if (cfRequestInterval < 20) cfRequestInterval = 20;
if (ipCheckInterval < 10) ipCheckInterval = 10;

Logger.LogInfo($"CloudFlare request interval: {cfRequestInterval}s");
Logger.LogInfo($"IP check interval: {ipCheckInterval}s");

#endregion

#region Get nginx sites enabled

var sitesEnabledList = Nginx.GetSitesEnabledDomains();
if (sitesEnabledList.Count > 0)
{
    Logger.LogInfo($"Nginx sites-enabled domain(s): {string.Join(", ", sitesEnabledList)}");
}
else
{
    Logger.LogError("No sites-enabled found in nginx. Service terminated.");
    Environment.Exit(1);
}

#endregion

#region Get public IPv6

var publicIP = Network.GetPublicIPv6();
while (publicIP == null)
{
    Logger.LogError("Public IPv6 not detected.");
    Thread.Sleep(ipCheckInterval * 1000);
    publicIP = Network.GetPublicIPv6();
}

Logger.LogInfo($"Public IPv6: {publicIP}");

#endregion

#region Verify CloudFlare API Token

CloudFlareAPI.SetZoneAndToken(cfZoneID, cfApiToken);
if (CloudFlareAPI.VerifyApiToken())
{
    Logger.LogInfo("CloudFlare API Token is valid.");
}
else
{
    Logger.LogError("Invalid CloudFlare API Token. Service terminated.");
    Environment.Exit(1);
}

#endregion

#region Get CloudFlare AAAA DNS records

var recordList = CloudFlareAPI.GetDNSRecords(sitesEnabledList);

while (recordList.Count == 0)
{
    Logger.LogWarning($"No AAAA DNS record found in CloudFlare. Will try again in {cfRequestInterval}s.");
    Thread.Sleep(cfRequestInterval * 1000);
    recordList = CloudFlareAPI.GetDNSRecords(sitesEnabledList);
}

#endregion

#region Monitor IPv6 & update DNS record

CloudFlareAPI.UpdateDNSRecordsIP(recordList, publicIP);

Logger.LogInfo("Monitoring public IPv6 change...");

while (true)
{
    Thread.Sleep(ipCheckInterval * 1000);

    var newIP = Network.GetPublicIPv6();
    if (newIP != null)
    {
        if (!newIP.Equals(publicIP, StringComparison.OrdinalIgnoreCase))
        {
            Logger.LogInfo($"Public IPv6 changed to: {publicIP}");

            recordList = CloudFlareAPI.GetDNSRecords(sitesEnabledList);
            while (recordList.Count == 0)
            {
                Logger.LogWarning($"No AAAA DNS record found in CloudFlare. Will try again in {cfRequestInterval}s.");
                Thread.Sleep(cfRequestInterval * 1000);
                recordList = CloudFlareAPI.GetDNSRecords(sitesEnabledList);
            }

            var dnsUpdated = CloudFlareAPI.UpdateDNSRecordsIP(recordList, publicIP);
            while (!dnsUpdated)
            {
                Logger.LogError($"Failed updating AAAA DNS record(s) in CloudFlare. Will try again in {cfRequestInterval}s.");
                Thread.Sleep(cfRequestInterval * 1000);
                dnsUpdated = CloudFlareAPI.UpdateDNSRecordsIP(recordList, publicIP);
            }

            publicIP = newIP;
        }
    }
    else
    {
        Logger.LogError("Public IPv6 not detected.");
    }
}

#endregion