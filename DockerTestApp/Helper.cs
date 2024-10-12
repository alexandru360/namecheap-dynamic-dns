using System.Reflection;

namespace DockerTestApp;

public static class Helper
{
    public static dynamic GetAssemblyVersion()
    {
        // get the version number and all you can about the assembly and return them as json
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        var assemblyName = assembly.GetName().Name;
        var assemblyInfo = new
        {
            Name = assemblyName,
            Version = version.ToString(),
            Description = assembly.GetCustomAttribute<System.Reflection.AssemblyDescriptionAttribute>()?.Description,
            Company = assembly.GetCustomAttribute<System.Reflection.AssemblyCompanyAttribute>()?.Company,
            Product = assembly.GetCustomAttribute<System.Reflection.AssemblyProductAttribute>()?.Product,
            Configuration = assembly.GetCustomAttribute<System.Reflection.AssemblyConfigurationAttribute>()?.Configuration,
            FileVersion = assembly.GetCustomAttribute<System.Reflection.AssemblyFileVersionAttribute>()?.Version,
            InformationalVersion = assembly.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            Title = assembly.GetCustomAttribute<System.Reflection.AssemblyTitleAttribute>()?.Title,
            Culture = assembly.GetCustomAttribute<System.Reflection.AssemblyCultureAttribute>()?.Culture,
            BuildDate = System.IO.File.GetCreationTime(assembly.Location),
            LastWriteTime = System.IO.File.GetLastWriteTime(assembly.Location),
            Location = assembly.Location
        };

        return assemblyInfo;
    }
}