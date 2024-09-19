using StockTracker.Client.Tools;
using System.Runtime.CompilerServices;

namespace StockTracker.Client.Models.Settings
{
    public class StockTrackerApiOptions
    {
        [EnvironmentVariable("STOCKTRACKER_API_BASE_URL")]
        public string BaseURL { get; set; } = string.Empty;
    }

}