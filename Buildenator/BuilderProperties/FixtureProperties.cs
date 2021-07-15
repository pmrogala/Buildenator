using Buildenator.Abstraction;

namespace Buildenator
{

    internal class FixtureProperties
    {
        public FixtureProperties(string name, string @namespace, Abstraction.FixtureInterfacesStrategy strategy, string[] additionalNamespaces)
        {
            Name = name;
            Namespace = @namespace;
            Strategy = strategy;
            AdditionalNamespaces = additionalNamespaces;
        }

        public string Name { get; }
        public string Namespace { get; }
        public FixtureInterfacesStrategy Strategy { get; }
        public string[] AdditionalNamespaces { get; }

    }
}
