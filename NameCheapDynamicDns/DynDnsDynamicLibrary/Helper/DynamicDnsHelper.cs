using System.Net;
using System.Text.RegularExpressions;
using DynDnsDynamicLibrary.Config;
using Microsoft.Extensions.Options;
using Serilog;

namespace DynDnsDynamicLibrary.Helper;

public class DynamicDnsHelper(ILogger logger, IOptions<NamecheapConfig> config) : IDynamicDnsHelper
{
    private readonly string[] _hosts = config.Value.Hosts.Split(',');
    private readonly string _domain = config.Value.Domain;
    private readonly string _password = config.Value.Password;

    private string _hostsUpdated = "";
    private string _hostsUnchanged = "";

    public async Task UpdateDns()
    {
        // Get environment variables

        if (string.IsNullOrEmpty(_domain) || string.IsNullOrEmpty(_password))
        {
            logger.Information("Missing required environment variables.");
            return;
        }

        // Get current WAN IP
        string? newIp = await GetCurrentWanIp();

        if (string.IsNullOrEmpty(newIp))
        {
            logger.Information("Failed to retrieve WAN IP.");
            return;
        }

        foreach (var host in _hosts)
        {
            string fullDomain = (host == "@") ? _domain : $"{host}.{_domain}";

            // Get current IP of the domain
            string? currentIp = GetCurrentIp(fullDomain);

            if (newIp == currentIp)
            {
                _hostsUnchanged += $"{fullDomain}\n";
                continue;
            }

            // Update the DNS record
            string? response = await UpdateDnsRecord(host, newIp);

            // Process the response
            if (IsUpdateSuccessful(response))
            {
                _hostsUpdated += $"{fullDomain}\n";
            }
            else
            {
                logger.Error($"Error updating {fullDomain}. Response: {response}");
            }
        }

        // Log results
        LogResults(newIp);
    }

    private async Task<string?> GetCurrentWanIp()
    {
        using HttpClient client = new HttpClient();
        try
        {
            return await client.GetStringAsync("http://ipinfo.io/ip");
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
        string url = $"https://dynamicdns.park-your-domain.com/update?host={host}&domain={_domain}&password={_password}&ip={newIp}";

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
        if (!string.IsNullOrEmpty(_hostsUpdated))
        {
            logger.Error($"The following hosts were updated with IP {newIp}:\n{_hostsUpdated}");
        }

        if (!string.IsNullOrEmpty(_hostsUnchanged))
        {
            logger.Error($"The following hosts were unchanged:\n{_hostsUnchanged}");
        }

        if (string.IsNullOrEmpty(_hostsUpdated))
        {
            logger.Error("No hosts were updated.");
        }
    }
}