using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI.Controls;
using Content = SenseNet.ContentRepository.Content;

namespace SenseNet.Portal.DiscussionForum
{
    public class TopicView : UserControl
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

        private Repeater _topicBody;
        public Repeater TopicBody => _topicBody ?? (_topicBody = FindControl("TopicBody") as Repeater);

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            TopicBody.ItemDataBound += TopicBody_ItemDataBound;
        }

        private void TopicBody_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (!(e.Item.FindControl("ReplyLink") is ActionLinkButton replyLink))
                return;

            replyLink.ActionName = "Add";
            replyLink.NodePath = ContextElement.Path;
            replyLink.Parameters = new { ReplyTo = ((Content)e.Item.DataItem).Path, ContentTypeName = "ForumEntry" };
        }
    }
}
