using System;

namespace Buildenator
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message) { }
    }
}
