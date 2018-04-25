﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.i18n;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.UI.Controls
{
    public class SurveyRuleEditor : FieldControl
    {
        public string ControlPath { get; set; } = "/Root/System/SystemPlugins/ListView/SurveyRuleEditor.ascx";

        protected virtual bool ControlStateLoaded { get; set; }

        public SurveyRuleEditor()
        {
            InnerControlID = "InnerListView";
        }

        protected List<SurveyRule> SurveyRulesList { get; set; }

        protected ListView DataListView { get; set; }

        protected DropDownList DdlSurveyQuestions { get; set; }

        protected List<ChoiceFieldSetting> ChoiceFieldSettings;

        private string _selectedQuestion = "-1";

        protected override void OnInit(EventArgs e)
        {
            ControlStateLoaded = false;
            Page.RegisterRequiresControlState(this); // this calls LoadControlState

            if (!IsTemplated && !string.IsNullOrEmpty(ControlPath))
            {
                if (Page.LoadControl(ControlPath) is SurveyRuleUserControl c)
                {
                    c.QuestionSelected += RuleControl_QuestionSelected;
                    DdlSurveyQuestions = c.FindControlRecursive("ddlSurveyQuestion") as DropDownList;

                    DataListView = c.FindControlRecursive("InnerListView") as ListView;

                    if (!Page.IsPostBack || SurveyRulesList == null)
                        SurveyRulesList = new List<SurveyRule>();

                    SetQuestions();
                    Controls.Add(c);
                }
            }

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DataListView != null && DataListView.DataSource == null)
            {
                DataListView.DataSource = SurveyRulesList;
                DataListView.DataBind();
            }
        }

        protected void RuleControl_QuestionSelected(object sender, EventArgs e)
        {
            SurveyRulesList = GetDataFromList().ToList();
        }

        protected override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                if (savedState is object[] state && state.Length == 2)
                {
                    base.LoadControlState(state[0]);

                    if (state[1] != null)
                    {
                        SurveyRulesList = (List<SurveyRule>)state[1];
                        ControlStateLoaded = true;
                    }
                }
            }
            else
            {
                base.LoadControlState(null);
            }
        }

        protected override object SaveControlState()
        {
            var state = new object[2];

            state[0] = base.SaveControlState();
            state[1] = SurveyRulesList;

            return state;
        }

        private IEnumerable<SurveyRule> GetDataFromList()
        {
            var result = new List<SurveyRule>();

            foreach (var item in DataListView.Items)
            {
                var txt = ((Literal)item.FindControl("ltrAnswerName")).Text;
                var hid = ((HiddenField)item.FindControl("hidAnswerValue")).Value;
                var jmp = ((DropDownList)item.FindControl("ddlJumpToPage")).Text;

                result.Add(new SurveyRule(txt, hid, jmp, 0));
            }

            return result;
        }

        public override object GetData()
        {
            var result = string.Empty;
            var sw = new StringWriter();
            var ws = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };

            using (var writer = XmlWriter.Create(sw, ws))
            {
                writer.WriteStartElement("SurveyRule");
                writer.WriteAttributeString("Question", DdlSurveyQuestions.SelectedValue);

                foreach (var rule in GetDataFromList())
                {
                    rule.WriteXml(writer);
                }

                writer.WriteEndElement();
                writer.Flush();

                result = sw.ToString();
            }

            return result;
        }

        private void SetQuestions()
        {
            ChoiceFieldSettings = new List<ChoiceFieldSetting>();

            var currentSurvey = ContentRepository.Content.Create(PortalContext.Current.ContextNode);
            var customFields = currentSurvey.Fields["FieldSettingContents"] as ReferenceField;
            if (customFields == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Survey", "ReferenceFieldError") });
                return;
            }

            var questions = customFields.OriginalValue as List<Node>;
            if (questions == null)
            {
                Controls.Add(new Literal { Text = SenseNetResourceManager.Current.GetString("Survey", "OriginalValueError") });
                return;
            }

            var cfs = NodeType.GetByName("ChoiceFieldSetting");
            var choiceTypeIds = cfs.GetAllTypes().ToIdArray();

            var pbfs = NodeType.GetByName("PageBreakFieldSetting");

            DdlSurveyQuestions.Items.Add(new ListItem("Choose a question!", "-100"));

            for (var i = questions.Count - 1; i >= 0; i--)
            {
                var questionTypeId = questions[i].NodeType.Id;

                if (choiceTypeIds.Contains(questionTypeId))
                {
                    if (!(questions[i] is FieldSettingContent q))
                        continue;

                    DdlSurveyQuestions.Items.Add(
                        q.FieldSetting.DisplayName.Length > 15
                            ? new ListItem(q.FieldSetting.DisplayName.Substring(0, 15) + "...", q.Name)
                            : new ListItem(q.FieldSetting.DisplayName, q.Name));

                    if (q.FieldSetting is ChoiceFieldSetting chfs)
                        ChoiceFieldSettings.Add(chfs);
                }

                if (questions[i].NodeType == pbfs)
                    continue;
            }
        }

        public override void SetData(object data)
        {
            // saving the field
            if (ControlStateLoaded)
                return;

            // loading the field
            // TODO: set question and page jumping values regarding to the following data:
            string data2 = data as string;

            if (string.IsNullOrEmpty(data2))
                return;

            data2 = data2.Replace("&lt;", "<").Replace("&gt;", ">");
            ParseRules(data2);
        }

        private void ParseRules(string xml)
        {
            var survey = ContentRepository.Content.Create(PortalContext.Current.ContextNode);
            var customFields = survey.Fields["FieldSettingContents"] as ReferenceField;
            var questions = customFields.OriginalValue as List<Node>;
            var pageBreaks = (from q in questions where q.NodeType.Name == "PageBreakFieldSetting" select q).Count();

            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch
            {
                return;
            }

            _selectedQuestion = doc.DocumentElement.GetAttribute("Question");

            if (_selectedQuestion == "-100")
            {
                DataListView.DataSource = null;
                DataListView.DataBind();
                return;
            }

            foreach (XPathNavigator node in doc.DocumentElement.CreateNavigator().SelectChildren(XPathNodeType.Element))
            {
                var answer = node.GetAttribute("Answer", "");
                var answerId = node.GetAttribute("AnswerId", "");
                var jumpToPage = node.Value;
                SurveyRulesList.Add(new SurveyRule(answer, answerId, jumpToPage, pageBreaks));
            }

            var chfs = ChoiceFieldSettings.FirstOrDefault(fs => fs.Name == _selectedQuestion);
            var allowExtra = chfs?.AllowExtraValue != null && chfs.AllowExtraValue.Value;

            if (allowExtra && SurveyRulesList.All(sr => sr.AnswerId != SurveyRule.EXTRAVALUEID))
                SurveyRulesList.Add(new SurveyRule(SurveyRule.GetExtraValueText(), SurveyRule.EXTRAVALUEID, "", pageBreaks));
            else if (!allowExtra)
                SurveyRulesList.RemoveAll(sr => sr.AnswerId == SurveyRule.EXTRAVALUEID);

            DdlSurveyQuestions.SelectedValue = _selectedQuestion;

            DataListView.DataSource = SurveyRulesList;
            DataListView.DataBind();
        }
    }
}