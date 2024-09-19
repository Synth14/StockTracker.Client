using StockTracker.Client.Tools;
using System.Text.Json.Serialization;

namespace StockTracker.Client.Models.Settings
{
    public class OIDCOptions
    {
        [EnvironmentVariable("OIDC_AUTHORITY")]
        public string Authority { get; set; }

        [EnvironmentVariable("OIDC_CLIENT_ID")]
        public string ClientId { get; set; }

        [EnvironmentVariable("OIDC_WELL_KNOWN")]
        public string WellKnown { get; set; }

        [EnvironmentVariable("OIDC_SCOPES")]
        public List<string> Scopes { get; set; }
    }
}