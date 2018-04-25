using System;
using System.Collections.Generic;
using SenseNet.Portal.UI;

namespace SenseNet.Search.Indexing
{
    public class WikiReferencedTitlesIndexHandler : LongTextIndexHandler
    {
        public override IEnumerable<IndexField> GetIndexFields(IIndexableField snField, out string textExtract)
        {
            var data = snField.GetData() as string ?? string.Empty;
            var titles = data.Split(new[] { WikiTools.REFERENCEDTITLESFIELDSEPARATOR }, 100000, StringSplitOptions.RemoveEmptyEntries);

            textExtract = string.Empty;

            return CreateField(snField.Name, titles);
        }
    }
}
