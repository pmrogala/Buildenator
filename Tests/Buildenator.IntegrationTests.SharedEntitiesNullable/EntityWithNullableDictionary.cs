using System.Collections.Generic;

namespace Buildenator.IntegrationTests.SharedEntitiesNullable
{
    /// <summary>
    /// Entity used to test nullable dictionary handling in generated builders.
    /// Tests various nullable dictionary types.
    /// </summary>
    public class EntityWithNullableDictionary
    {
        /// <summary>
        /// Nullable Dictionary property
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Nullable IDictionary property
        /// </summary>
        public IDictionary<int, string>? Items { get; set; }

        /// <summary>
        /// Nullable IReadOnlyDictionary property
        /// </summary>
        public IReadOnlyDictionary<string, object>? Settings { get; set; }

        /// <summary>
        /// Another nullable Dictionary property
        /// </summary>
        public Dictionary<string, int>? Scores { get; set; }

        /// <summary>
        /// Simple non-dictionary property for comparison
        /// </summary>
        public string? Name { get; set; }
    }
}
