namespace Buildenator.IntegrationTests.SharedEntities;

public class PostBuildEntity
{
    public int Entry { get; set; } = 1;

    public string NotReachableProperty { get; private set; } = string.Empty;
}