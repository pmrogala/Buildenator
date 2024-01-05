namespace Buildenator.IntegrationTests.SharedEntitiesNullable
{
    public class PostBuildEntity
    {
        public int Entry { get; set; } = 1;

        public string NotReachableProperty { get; private set; } = string.Empty;

        public string ReachableProperty { get; set; } = string.Empty;
    }
}
