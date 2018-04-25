using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls.WebParts;
using System.ComponentModel;
using System.Web.UI;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository.Workspaces;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.Portlets
{
    public class WorkspaceKPIPortlet : CacheablePortlet
    {
        private const string WorkspaceKPIPortletClass = "WorkspaceKPIPortlet";

        /* ====================================================================================================== Constants */
        private const string kpiViewPath = "/Root/Global/renderers/KPI/WorkspaceKPI";


        /* ====================================================================================================== Properties */
        [WebBrowsable(true)]
        [Personalizable(true)]
        [LocalizedWebDisplayName(WorkspaceKPIPortletClass, "Prop_ViewName_DisplayName")]
        [LocalizedWebDescription(WorkspaceKPIPortletClass, "Prop_ViewName_Description")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(DropDownPartField), typeof(IEditorPartField))]
        [DropDownPartOptions("InFolder:\"" + kpiViewPath + "\"")]
        public string ViewName { get; set; }


        /* ====================================================================================================== Constructor */
        public WorkspaceKPIPortlet()
        {
            Name = "$WorkspaceKPIPortlet:PortletDisplayName";
            Description = "$WorkspaceKPIPortlet:PortletDescription";
            Category = new PortletCategory(PortletCategoryType.KPI);

            HiddenProperties.AddRange(new[] { "SkinPreFix", "Renderer" });
        }


        /* ====================================================================================================== Methods */
        protected override void CreateChildControls()
        {
            // check if placed under a workspace
            if (!(PortalContext.Current.ContextNode is Workspace))
            {
                Controls.Clear();
                Controls.Add(new Label {Text = "This portlet is only operational in a workspace context!"});
                return;
            }

            // load view
            UserControl view = null;
            if (!string.IsNullOrEmpty(ViewName))
                view = Page.LoadControl(RepositoryPath.Combine(kpiViewPath, ViewName)) as UserControl;

            if (view != null)
                Controls.Add(view);
            else
                Controls.Add(new Label { Text = "No KPI view is loaded" });


            ChildControlsCreated = true;
        }
    }
}
