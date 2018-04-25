using SenseNet.Search;

namespace SenseNet.Compatibility
{
    /// <summary>Holds safe queries in public static readonly string properties.</summary>
    internal class SafeQueries : ISafeQueryHolder
    {
        /// <summary>Returns with the following query: "+Name:'*.xslt' +TypeIs:File .SORT:Path .AUTOFILTERS:OFF"</summary>
        public static string PreloadXslt => "+Name:'*.xslt' +TypeIs:File .SORT:Path .AUTOFILTERS:OFF";

        /// <summary>Returns with the following query: "+InTree:@0 +Depth:@1 .AUTOFILTERS:OFF"</summary>
        public static string PreloadContentTemplates => "+InTree:@0 +Depth:@1 .AUTOFILTERS:OFF";

        public static string PreloadControls => "+Name:\"*.ascx\" -Path:'/Root/Global/renderers/MyDataboundView.ascx' -Path:*/celltemplates* .SORT:Path .AUTOFILTERS:OFF";

        /// <summary>Returns with the following query: ""</summary>
        public static string Resources => "+TypeIs:Resource";

        /// <summary>Returns with the following query: "+TypeIs:Resource +ModificationDate:>@0"</summary>
        public static string ResourcesAfterADate => "+TypeIs:Resource +ModificationDate:>@0";
        /// <summary>Returns with the following query: "+TypeIs:calendarevent +RegistrationForm:@0"</summary>
        public static string CalendarEventsForRegistration => "+TypeIs:calendarevent +RegistrationForm:@0";


        // =============================================================== Wiki queries

        public static string WikiArticlesByDisplayName => "+TypeIs:WikiArticle +DisplayName:@0";
        public static string WikiArticlesByDisplayNameAndSubtree => "+TypeIs:WikiArticle +DisplayName:@0 +InTree:@1";
        public static string WikiArticlesByPath => "+TypeIs:WikiArticle +Path:@0";
        public static string WikiArticlesByReferenceTitlesAndSubtree => "+TypeIs:WikiArticle +ReferencedWikiTitles:@0 +InTree:@1";
    }
}
