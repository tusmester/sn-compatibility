using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.i18n;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.UI.Controls
{
    public class SurveyRuleUserControl : System.Web.UI.UserControl
    {
        public event EventHandler QuestionSelected;

        public void SurveyQuestionSelected(object sender, EventArgs e)
        {
            var ddlSurveyQuestion = sender as DropDownList;
            var listView = Page.FindControlRecursive("InnerListView") as ListView;

            if (ddlSurveyQuestion == null || listView == null)
                return;

            if (ddlSurveyQuestion.SelectedValue == "-1" || ddlSurveyQuestion.SelectedValue == "-100")
            {
                listView.DataSource = null;
                listView.DataBind();
                QuestionSelected?.Invoke(this, EventArgs.Empty);

                return;
            }

            var survey = ContentRepository.Content.Create(PortalContext.Current.ContextNode);

            if (!(survey.Fields["FieldSettingContents"] is ReferenceField customFields))
                return;

            var questions = customFields.OriginalValue as List<ContentRepository.Storage.Node>;

            var selectedQuestion = from q in questions where q.Name == ddlSurveyQuestion.SelectedValue select q;
            var pageBreaks = (from q in questions where q.NodeType.Name == "PageBreakFieldSetting" select q).Count();
            var choiceFs = (ChoiceFieldSetting) ((FieldSettingContent) selectedQuestion.First()).FieldSetting;
            var answers = choiceFs.Options;
            var rules = answers.Select(opt => new SurveyRule(opt.Text, opt.Value, "", pageBreaks)).ToList();

            if (choiceFs.AllowExtraValue.HasValue && choiceFs.AllowExtraValue.Value && rules.All(sr => sr.AnswerId != SurveyRule.EXTRAVALUEID))
            {
                // add extra value as a possible option
                rules.Add(new SurveyRule(SurveyRule.GetExtraValueText(), SurveyRule.EXTRAVALUEID, "", pageBreaks));
            }

            listView.DataSource = rules;
            listView.DataBind();

            QuestionSelected?.Invoke(this, EventArgs.Empty);
        }

        public void ListItemDataBound(object sender, ListViewItemEventArgs e)
        {
            var dataItem = e.Item as ListViewDataItem;
            if (dataItem == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Survey", "ListViewDataItemError") });
                return;
            }

            var surveyRule = dataItem.DataItem as SurveyRule;
            if (surveyRule == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Survey", "SurveyRuleError") });
                return;
            }

            var tempAnswer = surveyRule.Answer;
            if (tempAnswer.Length > 15)
            {
                tempAnswer = tempAnswer.Substring(0, 15) + "...";
            }

            var aNameLiteral = dataItem.FindControl("ltrAnswerName") as Literal;
            if (aNameLiteral != null)
                aNameLiteral.Text = tempAnswer;

            var aValueHidden = dataItem.FindControl("hidAnswerValue") as HiddenField;
            if (aValueHidden != null)
                aValueHidden.Value = surveyRule.AnswerId;

            var ddl = dataItem.FindControl("ddlJumpToPage") as DropDownList;
            if (ddl == null)
                throw new Exception("Cannot find control with ID ddlJumpToPage!");

            for (var i = 1; i <= surveyRule.Pages; i++)
            {
                ddl.Items.Add(new ListItem(i.ToString(), i.ToString()));
            }

            ddl.Items.Add(new ListItem("Finish", "-1"));
            ddl.Text = surveyRule.JumpToPage;
        }
    }
}
