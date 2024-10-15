using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using DynDnsDynamicLibrary.Config;
using DynDnsDynamicLibrary.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

namespace DynDnsDynamicLibrary.Helper;

public class DynamicDnsHelper(ILogger logger, IOptions<NamecheapConfig> config) : IDynamicDnsHelper
{
    private readonly string[] _hosts = config.Value.Hosts.Split(',');
    private readonly string _domain = config.Value.Domain;
    private readonly string _password = config.Value.Password;
    private readonly string _updateDomainsApiUrl = config.Value.UpdateDomainsApiUrl;
    private readonly string _ipCheckUrl = config.Value.IpCheckUrl;

    private DynamicDnsReportModel? _hostsChanges = new ();

    public async Task<DynamicDnsReportModel?> UpdateDns()
    {
        _hostsChanges = new ();
        
        // Get environment variables
        if (string.IsNullOrEmpty(_domain) || string.IsNullOrEmpty(_password))
        {
            logger.Error("Missing required environment variables.");
            return null;
        }

        // Get current WAN IP
        string? newIp = await GetCurrentWanIp();

        if (string.IsNullOrEmpty(newIp))
        {
            logger.Information("Failed to retrieve WAN IP.");
            return null;
        }

        foreach (var host in _hosts)
        {
            string fullDomain = (host == "@") ? _domain : $"{host}.{_domain}";

            // Get current IP of the domain
            string? currentIp = GetCurrentIp(fullDomain);

            if (newIp == currentIp)
            {
                _hostsChanges!.HostsUnchanged.Add(fullDomain);
                continue;
            }

            // Update the DNS record
            string? response = await UpdateDnsRecord(host, newIp);

            // Process the response
            if (IsUpdateSuccessful(response))
            {
                _hostsChanges!.HostsUpdated.Add(fullDomain);
            }
            else
            {
                logger.Information($"Updating {fullDomain}. Response: {JsonConvert.SerializeObject(response)}");
            }
        }

        // Log results
        LogResults(newIp);
        
        // return updates
        return _hostsChanges;
    }

    private async Task<string?> GetCurrentWanIp()
    {
        using HttpClient client = new HttpClient();
        try
        {
            var response = await client.GetStringAsync(_ipCheckUrl);
            logger.Information($"Current ip found: {response}");
            return response;
        }
        catch (Exception ex)
        {
            logger.Error($"Error fetching WAN IP: {ex.Message}");
            return null;
        }
    }

    private string? GetCurrentIp(string domain)
    {
        try
        {
            var addresses = Dns.GetHostAddresses(domain);
            return addresses.Length > 0 ? addresses[0].ToString() : null;
        }
        catch (Exception ex)
        {
            logger.Error($"Error resolving DNS for {domain}: {ex.Message}");
            return null;
        }
    }

    private async Task<string?> UpdateDnsRecord(string host, string? newIp)
    {
        var urlBuilder = new StringBuilder(_updateDomainsApiUrl);
        urlBuilder.Replace("{host}", host)
            .Replace("{domain}", _domain)
            .Replace("{password}", _password)
            .Replace("{newIp}", newIp);

        string url = urlBuilder.ToString();

        using HttpClient client = new HttpClient();
        try
        {
            return await client.GetStringAsync(url);
        }
        catch (Exception ex)
        {
            logger.Error($"Error updating DNS for {host}: {ex.Message}");
            return null;
        }
    }

    private bool IsUpdateSuccessful(string? response)
    {
        Regex regex = new Regex("<ErrCount>([0-1])</ErrCount>");
        Match match = regex.Match(response!);
        return match.Success && match.Groups[1].Value == "0";
    }

    private void LogResults(string? newIp)
    {
        if (!Convert.ToBoolean(_hostsChanges?.HostsUpdated.Count.Equals(0)))
        {
            logger.Information($"The following hosts were updated with IP {newIp}: {JsonConvert.SerializeObject(_hostsChanges.HostsUnchanged)}");
        }

        if (!Convert.ToBoolean(_hostsChanges?.HostsUnchanged.Count.Equals(0)))
        {
            logger.Information($"The following hosts were unchanged: {JsonConvert.SerializeObject(_hostsChanges.HostsUnchanged)}");
        }

        if (Convert.ToBoolean(_hostsChanges?.HostsUpdated.Count.Equals(0)))
        {
            logger.Information("No hosts were updated.");
        }
    }
}