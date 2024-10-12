namespace DynDnsDynamicLibrary.Models;

[Serializable]
public class DynamicDnsReportModel
{
    public IList<string> HostsUpdated = new List<string>();
    public IList<string> HostsUnchanged = new List<string>();
}