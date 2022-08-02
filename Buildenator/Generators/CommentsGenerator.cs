namespace Buildenator.Generators
{
    internal static class CommentsGenerator
    {
        internal static string GenerateSummaryComment(string text) => $@"
        /// <summary>
        /// {text}
        /// </summary>
";
        internal static string GenerateSummaryOverrideComment()
            => GenerateSummaryComment("You can \"override\" it by writing the definition in your part of the builder.");
    }
}