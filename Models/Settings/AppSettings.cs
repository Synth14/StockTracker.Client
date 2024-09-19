using System.Text.Json.Serialization;

namespace StockTracker.Client.Models.Settings
{
    public class AppSettings
    {
        public OIDCOptions OIDC { get; set; } = new OIDCOptions();

        [JsonPropertyName("StockTracker.API")]
        public StockTrackerApiOptions StockTrackerAPI { get; set; } = new StockTrackerApiOptions();
    }
}
