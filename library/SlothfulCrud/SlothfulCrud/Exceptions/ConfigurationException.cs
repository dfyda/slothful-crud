namespace SlothfulCrud.Exceptions
{
    public class ConfigurationException : ApplicationException
    {
        public ConfigurationException()
        {
        }
        
        public ConfigurationException(string message) : base(message)
        {
        }
    }
}