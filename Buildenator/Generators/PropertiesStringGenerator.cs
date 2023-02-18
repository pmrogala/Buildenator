using System.Linq;
using System.Text;
using Buildenator.CodeAnalysis;
using Buildenator.Configuration;
using Buildenator.Configuration.Contract;

namespace Buildenator.Generators;

internal sealed class PropertiesStringGenerator
{
	private readonly IBuilderProperties _builder;
	private readonly IEntityToBuild _entity;

	public PropertiesStringGenerator(IBuilderProperties builder, IEntityToBuild entity)
	{
		_builder = builder;
		_entity = entity;
	}

	public string GeneratePropertiesCode()
	{
		var properties = _entity.GetAllUniqueSettablePropertiesAndParameters();

		if (_builder.ShouldGenerateMethodsForUnreachableProperties)
		{
			properties = properties.Concat(_entity.GetAllUniqueNotSettablePropertiesWithoutConstructorsParametersMatch()).ToList();
		}

		var output = new StringBuilder();

		foreach (var typedSymbol in properties.Where(IsNotYetDeclaredField))
		{
			output.AppendLine($@"        private {typedSymbol.GenerateLazyFieldType()} {typedSymbol.UnderScoreName};");
		}

		foreach (var typedSymbol in properties.Where(IsNotYetDeclaredMethod))
		{
			output.AppendLine($@"

        {GenerateMethodDefinition(typedSymbol)}");

		}

		return output.ToString();

		bool IsNotYetDeclaredField(ITypedSymbol x) => !_builder.Fields.TryGetValue(x.UnderScoreName, out _);

		bool IsNotYetDeclaredMethod(ITypedSymbol x) => !_builder.BuildingMethods.TryGetValue(CreateMethodName(x), out var method)
		                                               || !(method.Parameters.Length == 1 && method.Parameters[0].Type.Name == x.TypeName);
	}

	private string GenerateMethodDefinition(ITypedSymbol typedSymbol)
		=> $@"{GenerateMethodDefinitionHeader(typedSymbol)}
        {{
            {GenerateValueAssignment(typedSymbol)};
            return this;
        }}";

	private string GenerateMethodDefinitionHeader(ITypedSymbol typedSymbol)
		=> $"public {_builder.FullName} {CreateMethodName(typedSymbol)}({typedSymbol.GenerateMethodParameterDefinition()})";

	private static string GenerateValueAssignment(ITypedSymbol typedSymbol)
		=> typedSymbol.IsMockable()
			? $"{DefaultConstants.SetupActionLiteral}({typedSymbol.UnderScoreName})"
			: $"{typedSymbol.UnderScoreName} = new Nullbox<{typedSymbol.TypeFullName}>({DefaultConstants.ValueLiteral})";

	private string CreateMethodName(ITypedSymbol property) => $"{_builder.BuildingMethodsPrefix}{property.SymbolPascalName}";
}