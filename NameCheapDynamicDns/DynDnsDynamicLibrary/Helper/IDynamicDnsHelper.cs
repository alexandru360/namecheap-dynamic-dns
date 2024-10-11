using System.Threading.Tasks;

namespace DynDnsDynamicLibrary;

public interface IDynamicDnsHelper
{
    public Task UpdateDns();
}