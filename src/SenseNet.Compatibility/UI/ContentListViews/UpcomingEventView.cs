using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Content=SenseNet.ContentRepository.Content;
using SenseNet.Portal.UI.Controls;

namespace SenseNet.Portal.UI.ContentListViews
{
    public class UpcomingEventView : ListView
    {
        private IEnumerable<Content> _contentList;
        private Calendar _calendar;
        private Calendar CalendarControl => _calendar ?? (_calendar = FindControl("CalendarControl") as Calendar);

        private Repeater _details;
        private Repeater DetailsControl
        {
            get
            {
                if (_details == null)
                {
                    _details = FindControl("DetailsControl") as Repeater;

                    if (_details != null)
                        _details.ItemDataBound += Details_ItemDataBound;
                }

                return _details;
            }
        }

        protected void Details_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Header || CalendarControl == null) 
                return;

            if (!(e.Item.FindControl("DateLabel") is Label dateLabel)) 
                return;

            dateLabel.Text = CalendarControl.SelectedDate.ToShortDateString();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (CalendarControl == null)
                return;
            
            CalendarControl.DayRender += CalendarControl_DayRender;
            CalendarControl.SelectionChanged += CalendarControl_SelectionChanged;
           
            _contentList = ViewDataSource.Select(DataSourceSelectArguments.Empty) ?? new List<Content>();
        }
        
        protected void CalendarControl_SelectionChanged(object sender, EventArgs e)
        {
            if (!(sender is Calendar calendar))
                return;

            if (DetailsControl == null)
                return;

            DetailsControl.DataSource = GetEvents(calendar.SelectedDate);
            DetailsControl.DataBind();
        }
        protected void CalendarControl_DayRender(object sender, DayRenderEventArgs e)
        {
            var scm = ScriptManager.GetCurrent(Page);
            
            if (scm != null && scm.IsInAsyncPostBack)
                return;

            e.Day.IsSelectable = HasEvent(e.Day.Date);

            var addLink = new ActionLinkButton
            {
                ActionName = "Add",
                NodePath = ContextNode.Path,
                ParameterString = "StartDate=" + e.Day.Date + ";ContentTypeName=CalendarEvent",
                IconVisible = false,
                Text = HttpContext.GetGlobalResourceObject("Renderers", "Add") as string,
                ToolTip = (HttpContext.GetGlobalResourceObject("Renderers", "AddNewEvent") as string) + e.Day.Date.ToString("MMMM dd")
            };
            e.Cell.Controls.Add(addLink);

        }
        
        private bool HasEvent(DateTime currentDate)
        {
            return GetEvents(currentDate).Any();
        }

        private IEnumerable<Content> GetEvents(DateTime matchDate)
        {
            return from c in _contentList
                   where DateTime.Compare(Convert.ToDateTime(c.Fields["StartDate"].GetData()).Date, matchDate.Date) == 0
                   select c;
        }
    }
}
