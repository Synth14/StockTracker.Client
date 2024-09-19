namespace StockTracker.Client.Tools
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EnvironmentVariableAttribute : Attribute
    {
        public string Name { get; }
        public EnvironmentVariableAttribute(string name) => Name = name;
    }
}
