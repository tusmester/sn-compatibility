//using System;
//using System.Linq;
//using SenseNet.ContentRepository.Storage.Security;
//using SenseNet.Diagnostics;
//using SenseNet.Portal.Portlets;
//using System.Text;
//using System.Web.Mvc;

// ======================================================================
// This class has been removed to avoid a direct MVC reference, and also
// because the old tagging feature is not supported in SN7.
// ======================================================================


//namespace SenseNet.Portal.ContentExplorer
//{
//    public class ContTagManagerController : Controller
//    {
//        public string returnTags(string q)
//        {
//            var sbBuilder = new StringBuilder();
//            var allTags = TagManager.GetAllTags(q, null);

//            foreach (var tags in allTags)
//            {
//                sbBuilder.Append(tags + "\n");
//            }
//            return sbBuilder.ToString().Trim(new[] { '\n' });
//        }

//        public ActionResult GetTags(string q)
//        {
//            // if nothing has been typed in the search input field
//            if (string.IsNullOrEmpty(q))
//            {
//                return Json(string.Empty, JsonRequestBehavior.AllowGet);
//            }
//            //returns the tags separetad with '\n' in Json 
//            return Json(returnTags(q), JsonRequestBehavior.AllowGet);
//        }

//        public ActionResult AddTag(int? id, string tag)
//        {
//            using (new SystemAccount())
//            {
//                if (!string.IsNullOrEmpty(tag) && !TagManager.IsBlacklisted(tag, null) && id.HasValue)
//                {
//                    tag = tag.ToLower();
//                    var content = ContentRepository.Content.Load(id.Value);
//                    if (content != null && !content.Fields["Tags"].OriginalValue.ToString().Split(' ').Contains(tag))
//                    {
//                        content.Fields["Tags"].SetData(content.Fields["Tags"].OriginalValue + " " + tag);
//                        try
//                        {
//                            content.Save();
//                        }
//                        catch (Exception ex) //logged
//                        {
//                            SnLog.WriteException(ex);
//                        }
//                    }
//                }
//            }
//            return null;
//        }
//    }
//}