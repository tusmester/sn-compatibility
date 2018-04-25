using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.i18n;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.Portlets
{
    public class KPIViewDropDownPartField : TextBox, IEditorPartField
    {
        /* ====================================================================================================== Constants */


        /* ====================================================================================================== IEditorPartField */
        public EditorOptions Options { get; set; }

        public string EditorPartCssClass { get; set; }
        public string TitleContainerCssClass { get; set; }
        public string TitleCssClass { get; set; }
        public string DescriptionCssClass { get; set; }
        public string ControlWrapperCssClass { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PropertyName { get; set; }
        public void RenderTitle(HtmlTextWriter writer)
        {
            writer.Write(@"<div class=""{0}""><span class=""{1}"" title=""{5}{6}"">{2}</span><br/><span class=""{3}"">{4}</span></div>", TitleContainerCssClass, TitleCssClass, Title, DescriptionCssClass, Description, SenseNetResourceManager.Current.GetString("PortletFramework", "PortletProperty"), PropertyName);
        }
        public void RenderDescription(HtmlTextWriter writer)
        {
        }

        /* ====================================================================================================== Properties */
        private DropDownPartOptions _dropdownOptions;
        public DropDownPartOptions DropdownOptions
        {
            get
            {
                if (_dropdownOptions == null)
                {
                    _dropdownOptions = Options as DropDownPartOptions ?? new DropDownPartOptions();
                }
                return _dropdownOptions;
            }
        }


        /* ====================================================================================================== Methods */
        protected override void Render(HtmlTextWriter writer)
        {
            var clientId = string.Concat(ClientID, "Div");
            string htmlPart2 = @"<div class=""{0}"" id=""{1}"">";
            writer.Write(htmlPart2, EditorPartCssClass, clientId);
            RenderTitle(writer);


            // render dropdown
            var controlCss = ControlWrapperCssClass;
            if (!string.IsNullOrEmpty(DropdownOptions.CustomControlCss))
                controlCss = string.Concat(controlCss, " ", DropdownOptions.CustomControlCss);

            writer.Write(@"<div class=""{0}"">", controlCss);
            writer.Write("<select></select>");


            // render textbox
            writer.Write(@"<div style=""display:none;"">");
            base.Render(writer);
            writer.Write("</div>");
            writer.Write("</div>");

            writer.Write("</div>");

        }
    }
}