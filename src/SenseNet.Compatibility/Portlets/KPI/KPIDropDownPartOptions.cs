using System;

namespace SenseNet.Portal.UI.PortletFramework
{
    [AttributeUsage(AttributeTargets.Property)]
    public class KPIDropDownPartOptions : EditorOptions
    {
        /* ================================================================================================================= Properties */
        public string Query { get; set; }
        public string MasterDropDownCss { get; set; }


        /* ================================================================================================================= Common constructors */
        public KPIDropDownPartOptions() {}
        public KPIDropDownPartOptions(string query, string masterDropDownCss)
        {
            Query = query;
            MasterDropDownCss = masterDropDownCss;
        }
    }
}
