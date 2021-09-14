﻿using Buildenator.Abstraction;

namespace Buildenator
{
    internal class FixtureProperties
    {
        public FixtureProperties(
            string name,
            string createSingleFormat,
            string? constructorParameters,
            string? additionalConfiguration, 
            FixtureInterfacesStrategy strategy,
            string[] additionalNamespaces)
        {
            Name = name;
            CreateSingleFormat = createSingleFormat;
            ConstructorParameters = constructorParameters;
            AdditionalConfiguration = additionalConfiguration;
            Strategy = strategy;
            AdditionalNamespaces = additionalNamespaces;
        }

        public string Name { get; }
        public string CreateSingleFormat { get; }
        public string? ConstructorParameters { get; }
        public string? AdditionalConfiguration { get; }
        public FixtureInterfacesStrategy Strategy { get; }
        public string[] AdditionalNamespaces { get; }

    }
}
