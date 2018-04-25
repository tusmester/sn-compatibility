using System;
using System.Linq;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.i18n;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.Virtualization;
using System.Web.UI.WebControls;
using SenseNet.Search;

namespace SenseNet.Portal.UI
{
    public class SurveyGenericContentView : GenericContentView
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var parent = ContentRepository.Content.Create(Content.ContentHandler.Parent);
            if (Content.ContentHandler.Parent is ContentList survey && !survey.FieldSettingContents.Any())
            {
                if (this.FindControlRecursive("PlcEmptySurvey") is PlaceHolder plcEmptySurvey)
                {
                    plcEmptySurvey.Visible = true;

                    if (this.FindControlRecursive("LblNoQuestion") is Label myLabel)
                    {
                        myLabel.Visible = true;
                        myLabel.Text = SenseNetResourceManager.Current.GetString("Survey", "NoQuestion");
                    }
                }

                var gfControl = this.FindControlRecursive("GenericFieldControl1") as GenericFieldControl;
                if (gfControl?.Wizard == null) 
                    return;

                gfControl.Wizard.Visible = false;
                return;
            }

            if (Convert.ToBoolean(parent["EnableLifespan"]) &&
                (DateTime.UtcNow < Convert.ToDateTime(parent["ValidFrom"]) ||
                 DateTime.UtcNow > Convert.ToDateTime(parent["ValidTill"])))
            {
                Session["error"] = "invalid";
                Response.Redirect(PortalContext.Current.RequestedUri.GetLeftPart(UriPartial.Path));
            }

            if (Convert.ToBoolean(parent["EnableMoreFilling"])) 
                return;         

            if (ContentQuery.Query("+Type:surveyitem +InFolder:@0 +CreatedById:@1 .COUNTONLY", null, parent.Path, User.Current.Id).Count > 0)
            {
                Session["error"] = "morefilling";
                Response.Redirect(PortalContext.Current.RequestedUri.GetLeftPart(UriPartial.Path));
            }
        }
    }
}