using Microsoft.Extensions.Configuration;
using System;

namespace GuedesPlace.AzureTools.Configuration.Extensions;

public static class IConfigurationExtensions {

    public static void CheckConfigurationValueAvailable(this IConfiguration configuration, string valueName) {
        var value = configuration[valueName];
        if (string.IsNullOrEmpty(value)){
            throw new ArgumentNullException(valueName, "Configuration missing");
        }
    }
    public static void CheckConfigurationValuesAvailable(this IConfiguration configuration, ICollection<string> valueNames) {
        foreach(var valueName in valueNames) {
            CheckConfigurationValueAvailable(configuration,valueName);
        }
    }
}