using System;

namespace Buildenator.Exceptions
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message) { }
    }
}
