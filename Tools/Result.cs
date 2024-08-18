namespace StockTracker.Client.Tools
{
    public class Result
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
    public class Result<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string ErrorMessage { get; set; }
    }
}
