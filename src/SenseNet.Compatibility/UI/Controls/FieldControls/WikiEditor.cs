using System.Web.UI;
using SenseNet.ContentRepository.Workspaces;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:WikiEditor ID=\"WikiEditort1\" runat=server></{0}:WikiEditor>")]
    public class WikiEditor : RichText
    {
        public override object GetData()
        {
            return ControlMode == FieldControlControlMode.Edit ? 
                WikiTools.ConvertWikilinksToHtml(base.GetData() as string, Content.ContentHandler.Parent) : 
                base.GetData();
        }

        public override void SetData(object data)
        {
            base.SetData(ControlMode == FieldControlControlMode.Edit
                             ? WikiTools.ConvertHtmlToWikilinks(data as string)
                             : data);
        }

        protected override void InitImagePickerParams()
        {
            var ws = Workspace.GetWorkspaceForNode(ContentHandler);
            var imagesPath = RepositoryPath.Combine(ws.Path, "Images");
            var script = $"SN.tinymceimagepickerparams = {{ TreeRoots:['{imagesPath}','/Root'] }};";
            UITools.RegisterStartupScript("tinymceimagepickerparams", script, Page);
        }
    }
}
