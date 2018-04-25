using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.Portlets.ContentHandlers;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Diagnostics;
using Content = SenseNet.ContentRepository.Content;

namespace SenseNet.Portal.Portlets
{
    public class FormPortlet : PortletBase
    {
        private const string FormPortletClass = "FormPortlet";

        // -- Variables ---------------------------------------------------

        private Form _currentForm;
        private Content _cFormItem;
        protected ContentView _cvFormItem;

        // -- Properties ---------------------------------------------------

        [WebBrowsable(true)]
        [Personalizable(true)]
        [LocalizedWebDisplayName(FormPortletClass, "Prop_FormPath_DisplayName")]
        [LocalizedWebDescription(FormPortletClass, "Prop_FormPath_Description")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions()]
        [WebOrder(100)]
        public string FormPath { get; set; }

        private Form CurrentForm
        {
            get
            {
                if (_currentForm != null && _currentForm.Path == FormPath)
                    return _currentForm;

                if (!string.IsNullOrEmpty(FormPath))
                {
                    // Elevation: the portlet should have access to the current form,
                    // regardless of the current users permission level.
                    using (new SystemAccount())
                    {
                        _currentForm = Node.LoadNode(FormPath) as Form;
                    }
                }

                return _currentForm;
            }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView, PortletViewType.Ascx)]
        [WebOrder(110)]
        public string ContentViewPath { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [LocalizedWebDisplayName(FormPortletClass, "Prop_AfterSubmitViewPath_DisplayName")]
        [LocalizedWebDescription(FormPortletClass, "Prop_AfterSubmitViewPath_Description")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView)]
        [WebOrder(120)]
        public string AfterSubmitViewPath { get; set; }

        #region from changeset #19024

        [WebBrowsable(true), Personalizable(true)]
        [LocalizedWebDisplayName(FormPortletClass, "Prop_PermissionErrorViewPath_DisplayName")]
        [LocalizedWebDescription(FormPortletClass, "Prop_PermissionErrorViewPath_Description")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView)]
        [WebOrder(120)]
        public string PermissionErrorViewPath { get; set; }

        #endregion
        
        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        private bool _isContentValid;
        private int _formItemId;

        // -- Constructors ------------------------------------------------
        public FormPortlet()
        {
            Name = "$FormPortlet:PortletDisplayName";
            Description = "$FormPortlet:PortletDescription";
            Category = new PortletCategory(PortletCategoryType.Application);
            _isContentValid = false;

            HiddenProperties.Add("Renderer");
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.RegisterRequiresControlState(this);
        }

        protected override void LoadControlState(object savedState)
        {
            if (!(savedState is object[] data))
                base.LoadControlState(savedState);
            else if (data.Length != 3)
                base.LoadControlState(data);
            else
            {
                _isContentValid = Convert.ToBoolean(data[0]);
                _formItemId = Convert.ToInt32(data[1]);
                base.LoadControlState(data[2]);
            }
        }
        protected override object SaveControlState()
        {
            var data = new object[3];
            data[0] = _isContentValid;
            data[1] = _formItemId;
            data[2] = base.SaveControlState();
            return data;
        }


        protected override void CreateChildControls()
        {
            Controls.Clear();

            if (Page.IsPostBack && _isContentValid)
            {
                BuildAfterSubmitForm(null);
            }
            else
            {
                CreateControls();
            }
            
            ChildControlsCreated = true;
        }

        private void CreateControls()
        {
            if (CurrentForm != null)
            {
                if (!CurrentForm.Security.HasPermission(PermissionType.AddNew))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(PermissionErrorViewPath))
                        {
                            _cvFormItem = ContentView.Create(Content.Create(CurrentForm), Page, ViewMode.Browse, PermissionErrorViewPath);
                            Controls.Add(_cvFormItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        SnLog.WriteException(ex);
                        Controls.Clear();
                        Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
                    }
                }
                else
                {
                    CreateNewFormItem();
                }
            }
        }

        // -- Methods -----------------------------------------------------

        private void CreateNewFormItem()
        {
            if (CurrentForm == null)
                return;

            var act = CurrentForm.GetAllowedChildTypes();
            var allowedType = act.FirstOrDefault(ct => ct.IsInstaceOfOrDerivedFrom("FormItem"));
            var typeName = allowedType == null ? "FormItem" : allowedType.Name;

            _cFormItem = Content.CreateNew(typeName, CurrentForm, null);

            try
            {
                if (string.IsNullOrEmpty(ContentViewPath))
                    _cvFormItem = ContentView.Create(_cFormItem, Page, ViewMode.New);
                else
                    _cvFormItem = ContentView.Create(_cFormItem, Page, ViewMode.New, ContentViewPath);

                _cvFormItem.ID = "cvNewFormItem";
                _cvFormItem.Init += _cvFormItem_Init;
                _cvFormItem.UserAction += _cvFormItem_UserAction;

                Controls.Add(_cvFormItem);
            }
            catch (Exception ex) // logged
            {
                SnLog.WriteException(ex);
                Controls.Clear();
                Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
            }
        }

        private void _cvFormItem_UserAction(object sender, UserActionEventArgs e)
        {
            switch (e.ActionName)
            {
                case "save":
                    e.ContentView.UpdateContent();
                    _isContentValid = e.ContentView.Content.IsValid && e.ContentView.IsUserInputValid;
                    if (e.ContentView.IsUserInputValid && e.ContentView.Content.IsValid)
                    {
                        try
                        {
                            e.ContentView.Content.Save();

                            Controls.Clear();

                            _formItemId = e.ContentView.Content.Id;

                            BuildAfterSubmitForm(e.ContentView.Content);
                        }
                        catch (FormatException ex) // logged
                        {
                            SnLog.WriteException(ex);
                            e.ContentView.ContentException = new FormatException("Invalid format!", ex);
                        }
                        catch (Exception exc) // logged
                        {
                            SnLog.WriteException(exc);
                            e.ContentView.ContentException = exc;
                        }
                    }
                    break;
            }
        }

        private void BuildAfterSubmitForm(Content formitem)
        {
            if (!string.IsNullOrEmpty(AfterSubmitViewPath))
            {
                BuildAfterSubmitForm_ContentView(formitem);
            }
            else
            {
                Controls.Add(new LiteralControl(string.Concat("<div class=\"sn-form-submittext\">", CurrentForm.AfterSubmitText, "</div>")));
                var ok = new Button();
                ok.ID = "btnOk";
                ok.Text = "Ok";
                ok.CssClass = "sn-submit";
                ok.Click += ok_Click;
                Controls.Add(ok);
            }
        }

        private void BuildAfterSubmitForm_ContentView(Content formitem)
        {
            if (CurrentForm == null)
                return;

            if (formitem == null && _formItemId != 0)
            {
                formitem = Content.Load(_formItemId);
            }

            if (formitem != null)
            {
                _cFormItem = formitem;

                _cvFormItem = ContentView.Create(_cFormItem, Page, ViewMode.New, AfterSubmitViewPath);

                _cvFormItem.ID = "cvAfterSubmitFormItem";

                _cvFormItem.UserAction += _cvAfterSubmitFormItem_UserAction;

                Controls.Add(_cvFormItem);
            }
            else if (!string.IsNullOrEmpty(AfterSubmitViewPath))
            {
                Controls.Add(Page.LoadControl(AfterSubmitViewPath));
            }
        }

        private void _cvAfterSubmitFormItem_UserAction(object sender, UserActionEventArgs e)
        {
            switch (e.ActionName)
            {
                case "ok":
                    CreateControls();
                    break;
            }
        }

        private void ok_Click(object sender, EventArgs e)
        {
            _isContentValid = false;
            Controls.Clear();
            CreateControls();

            CallDone();
        }

        private void _cvFormItem_Init(object sender, EventArgs e)
        {
            if (!((sender as ContentView)?.FindControl("btnSave") is Button btnSave))
                return;

            btnSave.Text = "Send";
            btnSave.Focus();
        }
    }
}
