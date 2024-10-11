using System.Net;
using System.Text.RegularExpressions;

public class DynDnsUpdater
{
    private static string[] hosts;
    private static string domain;
    private static string password;
    private static string hostsUpdated = "";
    private static string hostsUnchanged = "";
    private readonly Serilog.ILogger _logger;

    public DynDnsUpdater(Serilog.ILogger logger)
    {
        _logger = logger;
    }

    public async Task UpdateDns()
    {
        // Get environment variables
        hosts = Environment.GetEnvironmentVariable("HOSTS")?.Split(' ') ?? Array.Empty<string>();
        domain = Environment.GetEnvironmentVariable("DOMAIN");
        password = Environment.GetEnvironmentVariable("PASSWORD");

        if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(password))
        {
            _logger.Error("Missing required environment variables.");
            return;
        }

        // Get current WAN IP
        string newIp = await GetCurrentWanIp();

        if (string.IsNullOrEmpty(newIp))
        {
            _logger.Error("Failed to retrieve WAN IP.");
            return;
        }

        foreach (var host in hosts)
        {
            string fullDomain = (host == "@") ? domain : $"{host}.{domain}";

            // Get current IP of the domain
            string currentIp = GetCurrentIp(fullDomain);

            if (newIp == currentIp)
            {
                hostsUnchanged += $"{fullDomain}\n";
                continue;
            }

            // Update the DNS record
            string response = await UpdateDnsRecord(host, newIp);

            // Process the response
            if (IsUpdateSuccessful(response))
            {
                hostsUpdated += $"{fullDomain}\n";
            }
            else
            {
                _logger.Error("Error updating {FullDomain}. Response: {Response}", fullDomain, response);
            }
        }

        // Log results
        LogResults(newIp);
    }

    private async Task<string> GetCurrentWanIp()
    {
        using HttpClient client = new HttpClient();
        try
        {
            return await client.GetStringAsync("http://ipinfo.io/ip");
        }
        catch (Exception ex)
        {
            _logger.Error("Error fetching WAN IP: {Message}", ex.Message);
            return null;
        }
    }

    private string GetCurrentIp(string domain)
    {
        try
        {
            var addresses = Dns.GetHostAddresses(domain);
            return addresses.Length > 0 ? addresses[0].ToString() : null;
        }
        catch (Exception ex)
        {
            _logger.Error("Error resolving DNS for {Domain}: {Message}", domain, ex.Message);
            return null;
        }
    }

    private async Task<string> UpdateDnsRecord(string host, string newIp)
    {
        string url = $"https://dynamicdns.park-your-domain.com/update?host={host}&domain={domain}&password={password}&ip={newIp}";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                return await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating DNS for {Host}: {Message}", host, ex.Message);
                return null;
            }
        }
    }

    private bool IsUpdateSuccessful(string response)
    {
        Regex regex = new Regex("<ErrCount>([0-1])</ErrCount>");
        Match match = regex.Match(response);
        return match.Success && match.Groups[1].Value == "0";
    }

    private void LogResults(string newIp)
    {
        if (!string.IsNullOrEmpty(hostsUpdated))
        {
            _logger.Information("The following hosts were updated with IP {NewIp}:\n{HostsUpdated}", newIp, hostsUpdated);
        }

        if (!string.IsNullOrEmpty(hostsUnchanged))
        {
            _logger.Information("The following hosts were unchanged:\n{HostsUnchanged}", hostsUnchanged);
        }

        if (string.IsNullOrEmpty(hostsUpdated))
        {
            _logger.Information("No hosts were updated.");
        }
    }
}