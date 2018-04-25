using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.i18n;

namespace SenseNet.Portal.UI
{
    public class VotingResultContentView : SingleContentView
    {
        public Dictionary<string, string> Result { get; set; }
        public int DecimalsInResult { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!(Node.LoadNode(Content.Id) is Voting voting))
            {
                Controls.Add(new Literal { Text = "Error with loading the content with ID: " + Content.Id });
                return;
            }

            var dt = new DataTable();

            dt.Columns.AddRange(new[]
            {
                new DataColumn("Question", typeof(string)),
                new DataColumn("Count", typeof(double))
            });

            var sum = voting.Result.Sum(item => Convert.ToDouble(item.Value));

            var formatString = string.Concat("N",
                DecimalsInResult < 0 || DecimalsInResult > 5
                    ? 0
                    : DecimalsInResult);

            foreach (var item in voting.Result)
            {
                var itemText = item.Key;
                if (SenseNetResourceManager.ParseResourceKey(itemText, out var classname, out var keyname))
                    itemText = SenseNetResourceManager.Current.GetString(classname, keyname);

                dt.Rows.Add(itemText,
                    sum == 0 || Convert.ToInt32(item.Value) == 0
                        ? 0.ToString()
                        : ((Convert.ToDouble(item.Value) / sum) * 100).ToString(formatString));
            }

            if (FindControl("ResultList") is ListView lv)
            {
                lv.DataSource = dt.DefaultView;
                lv.DataBind();
            }
        }
    }
}
