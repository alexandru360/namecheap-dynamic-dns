using System.Threading.Tasks;
using DynDnsDynamicLibrary.Models;

namespace DynDnsDynamicLibrary;

public interface IDynamicDnsHelper
{
    public Task<DynamicDnsReportModel?> UpdateDns();
}