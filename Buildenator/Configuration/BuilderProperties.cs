using Buildenator.Abstraction;
using Buildenator.Configuration.Contract;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Buildenator.Configuration
{
	internal sealed class BuilderProperties : IBuilderProperties
	{
		private readonly Dictionary<string, IMethodSymbol> _buildingMethods;
		private readonly Dictionary<string, IFieldSymbol> _fields;

		public BuilderProperties(INamedTypeSymbol builderSymbol, MakeBuilderAttributeInternal attributeData)
		{
			ContainingNamespace = builderSymbol.ContainingNamespace.ToDisplayString();
			Name = builderSymbol.Name;
			FullName = builderSymbol.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
			BuildingMethodsPrefix = attributeData.BuildingMethodsPrefix ?? DefaultConstants.BuildingMethodsPrefix;
			NullableStrategy = attributeData.NullableStrategy ?? NullableStrategy.Default;
			StaticCreator = attributeData.DefaultStaticCreator ?? true;
			ImplicitCast = attributeData.ImplicitCast ?? false;
			ShouldGenerateMethodsForUnreachableProperties = attributeData.GenerateMethodsForUnreachableProperties ?? false;

			if (string.IsNullOrWhiteSpace(BuildingMethodsPrefix))
				throw new ArgumentNullException(nameof(attributeData), "Prefix name shouldn't be empty!");

			_buildingMethods = new Dictionary<string, IMethodSymbol>();
			_fields = new Dictionary<string, IFieldSymbol>();
			var members = builderSymbol.GetMembers();
			foreach (var member in members)
			{
				switch (member)
				{
					case IMethodSymbol { MethodKind: MethodKind.Ordinary } method when method.Name.StartsWith(BuildingMethodsPrefix):
						_buildingMethods.Add(method.Name, method);
						break;
					case IMethodSymbol { MethodKind: MethodKind.Ordinary, Name: DefaultConstants.PostBuildMethodName }:
						IsPostBuildMethodOverriden = true;
						break;
					case IMethodSymbol { MethodKind: MethodKind.Constructor, Parameters.Length: 0, IsImplicitlyDeclared: false }:
						IsDefaultContructorOverriden = true;
						break;
					case IFieldSymbol field:
						_fields.Add(field.Name, field);
						break;
				}
			}
		}

		public string ContainingNamespace { get; }
		public string Name { get; }
		public string FullName { get; }
		public string BuildingMethodsPrefix { get; }
		public NullableStrategy NullableStrategy { get; }
		public bool StaticCreator { get; }
		public bool ImplicitCast { get; }
		public bool IsPostBuildMethodOverriden { get; }
		public bool IsDefaultContructorOverriden { get; }
		public bool ShouldGenerateMethodsForUnreachableProperties { get; }

		public IReadOnlyDictionary<string, IMethodSymbol> BuildingMethods => _buildingMethods;
		public IReadOnlyDictionary<string, IFieldSymbol> Fields => _fields;

	}
}