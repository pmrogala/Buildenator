using System.Collections.Generic;

namespace Buildenator.IntegrationTests.SharedEntities;

public class EntityWithMandatoryConstructorParameters
{
    public EntityWithMandatoryConstructorParameters(int lineNumber, string activity)
    {
        LineNumber = lineNumber;
        Activity = activity;
    }

    public int LineNumber { get; set; }
    public string Activity { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
}
