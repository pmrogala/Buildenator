using Buildenator.Abstraction;
using Buildenator.IntegrationTests.SharedEntities;
using System.Collections.Generic;
using System.Linq;

namespace Buildenator.IntegrationTests.Source.Builders;

[MakeBuilder(typeof(CustomBuildManyEntity))]
public partial class CustomBuildManyEntityBuilder
{
    private static int _idCounter = 0;

    public IEnumerable<CustomBuildManyEntity> BuildMany(int count = 3)
    {
        return Enumerable.Range(0, count).Select(i =>
        {
            var entity = Build();
            entity.Id = ++_idCounter;
            return entity;
        });
    }
}
