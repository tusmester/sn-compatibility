using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Portlets
{
    public class TagManagerApi: GenericApi
    {
        private static string returnTags(string q)
        {
            var sbBuilder = new StringBuilder();
            var allTags = TagManager.GetAllTags(q, null);

            foreach (var tags in allTags)
            {
                sbBuilder.Append(tags + "\n");
            }
            return sbBuilder.ToString().Trim(new[] { '\n' });
        }

        [ODataFunction]
        public static string GetTags(Content content, string query)
        {
            // if nothing has been typed in the search input field
            if (string.IsNullOrEmpty(query))
            {
                return string.Empty;
            }
            // returns the tags separetad with '\n' in Json 
            return returnTags(query);
        }

        [ODataAction]
        public static void AddTag(Content dummy, string tag, int id = 0)
        {
            using (new SystemAccount())
            {
                if (!string.IsNullOrEmpty(tag) && !TagManager.IsBlacklisted(tag, null) && id != 0)
                {
                    tag = tag.ToLower();
                    var content = Content.Load(id);
                    if (content != null && (content.Fields["Tags"].OriginalValue == null || !content.Fields["Tags"].OriginalValue.ToString().Split(' ').Contains(tag)))
                    {
                        content.Fields["Tags"].SetData(content.Fields["Tags"].OriginalValue + " " + tag);
                        try
                        {
                            content.Save();
                        }
                        catch (Exception ex) // logged
                        {
                            SnLog.WriteException(ex);
                        }
                    }
                }
            }
        }
    }
}
