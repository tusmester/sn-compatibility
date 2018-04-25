using System;
using System.Web.UI;
using SenseNet.ContentRepository.Fields;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:Rating ID=\"Rating1\" runat=\"server\"></{0}:Rating>")]
    public class Rating : FieldControl
    {
        private bool _setdata;
        private readonly RateControl _rateControl;
        public bool EnableAdminMode = false;
        public bool EnableHoverPanel = true;
        public bool EnableGroupSplit = true;

        public Rating()
        {
            _rateControl = new RateControl {ID = "rateControl"};
        }

        #region FieldControl overrides

        public override object GetData()
        {
            return Convert.ToBoolean(Content.Fields["IsRateable"].OriginalValue) ? _rateControl.VotesArray : VoteData.CreateVoteData(5,1);
        }

        public override void SetData(object data)
        {
            var rawData = data as VoteData;

            if (rawData != null)
            {
                _rateControl.VotesArray = rawData;
                _setdata = true;
            }
        }

        protected override void OnInit(EventArgs e)
        {

            base.OnInit(e);
            if (!_setdata)
            {
                var fs = Field.FieldSetting as RatingFieldSetting;
                _rateControl.MaxVotes = fs?.Range ?? 5;
                _rateControl.SplitVote = fs?.Split ?? 1;
            }

            _rateControl.ContentId = Field.Content.Id;
            _rateControl.EnableGroupSplit = EnableGroupSplit;
            _rateControl.EnableHoverPanel = EnableHoverPanel;
            _rateControl.Mode = ReadOnly
                                    ? RateControl.RateControlMode.ReadOnly
                                    : EnableAdminMode
                                          ? RateControl.RateControlMode.Admin
                                          : RateControl.RateControlMode.Normal;
            _rateControl.Enable = true;
            Controls.Add(_rateControl);

        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (Convert.ToBoolean(Content["IsRateable"]))
                RenderContents(writer);
        }

        #endregion
    }
}