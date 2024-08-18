namespace StockTracker.Client.Tools
{
    public class ApiException : Exception
    {
        public System.Net.HttpStatusCode StatusCode { get; }

        public ApiException(string message, System.Net.HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
