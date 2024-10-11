using DynDnsDynamicLibrary.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DynDnsDynamicLibrary;

public static class DynDnsDynamicLibraryConfiguration
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IDynamicDnsHelper, DynamicDnsHelper>();

        return services;
    }
}