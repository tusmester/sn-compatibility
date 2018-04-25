// ReSharper disable once CheckNamespace
// ReSharper disable RedundantTypeArgumentsOfMethod
namespace SenseNet.Configuration
{
    public class Journal : SnConfig
    {
        private const string SectionName = "sensenet/journal";

        public static bool CreateJournalItems { get; internal set; } = GetValue(SectionName, "CreateJournalItems", true);
    }
}
