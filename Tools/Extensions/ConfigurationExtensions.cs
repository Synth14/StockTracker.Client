using StockTracker.Client.Models.Settings;
using System.Reflection;

namespace StockTracker.Client.Tools.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection ConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettings = new AppSettings();
            configuration.Bind(appSettings);

            ApplyEnvironmentVariables(appSettings);

            services.AddSingleton(appSettings);
            services.AddSingleton(appSettings.OIDC);
            services.AddSingleton(appSettings.StockTrackerAPI);

            return services;
        }

        private static void ApplyEnvironmentVariables(object obj)
        {
            foreach (var prop in obj.GetType().GetProperties())
            {
                var attr = prop.GetCustomAttribute<EnvironmentVariableAttribute>();
                if (attr != null)
                {
                    var envValue = Environment.GetEnvironmentVariable(attr.Name);
                    if (!string.IsNullOrEmpty(envValue))
                    {
                        var convertedValue = Convert.ChangeType(envValue, prop.PropertyType);
                        prop.SetValue(obj, convertedValue);
                    }
                }

                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    ApplyEnvironmentVariables(prop.GetValue(obj));
                }
            }
        }
    }
}
