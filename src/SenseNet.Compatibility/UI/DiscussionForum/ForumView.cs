using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI.Controls;
using Content = SenseNet.ContentRepository.Content;

namespace SenseNet.Portal.DiscussionForum
{
    public class ForumView : UserControl
    {
        private Content _contextElement;
        public Content ContextElement
        {
            get
            {
                if (_contextElement == null)
                {
                    var ci = (ContextInfo)FindControl("ViewContext");
                    _contextElement = Content.Load(ci.Path);
                }

                return _contextElement;
            }
        }

        private Repeater _forumBody;
        public Repeater ForumBody => _forumBody ?? (_forumBody = FindControl("ForumBody") as Repeater);

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ForumBody.ItemDataBound += ForumBody_ItemDataBound;
        }

        private void ForumBody_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var myContent = (Content)e.Item.DataItem;

            if (e.Item.FindControl("BrowseLink") is ActionLinkButton browseLink)
            {
                browseLink.ActionName = "Browse";
                browseLink.NodePath = myContent.Path;
                browseLink.Text = HttpUtility.HtmlEncode(myContent.DisplayName);
            }

            if (e.Item.FindControl("PostNum") is Label numLabel)
                numLabel.Text = myContent.Children.Count().ToString();

            if (e.Item.FindControl("PostDate") is Label dateLabel)
            {
                var oldest = myContent.Children.OrderBy(n => n.CreationDate).FirstOrDefault() ?? myContent;

                dateLabel.Text = oldest.CreationDate.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
