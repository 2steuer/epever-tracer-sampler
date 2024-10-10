using System.Configuration;
using System.Security.Permissions;
using Microsoft.Extensions.Configuration;

public static class ConfigExtensions
{
    public static Dictionary<int, string> GetChannelMapping(this IConfiguration cfg, string key)
    {
        var data = cfg
            .GetSection(key)
            .Get<Dictionary<string, string>>();

        return data?.ToDictionary(
            kvp => int.Parse(kvp.Key),
            kvp => kvp.Value
        ) ?? throw new ConfigurationErrorsException("Channel mapping incorrectly formatted.");
    }
}