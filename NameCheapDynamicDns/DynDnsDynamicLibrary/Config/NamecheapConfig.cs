namespace DynDnsDynamicLibrary.Config;

public class NamecheapConfig
{
    public required string UpdateDomainsApiUrl { get; set; }
    public required string Hosts { get; set; }
    public required string Domain { get; set; }
    public required string Password { get; set; }
}