using System;
using System.Linq;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.Portlets.ContentHandlers
{
    [ContentHandler]
    public class SurveyList : ContentList
    {
        // ================================================================================ Constructors

        public SurveyList(Node parent) : this(parent, null) { }
        public SurveyList(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected SurveyList(NodeToken nt) : base(nt)	{ }

        // ================================================================================ Properties

        [RepositoryProperty(nameof(EmailList), RepositoryDataType.Text)]
        public string EmailList
        {
            get => base.GetProperty<string>(nameof(EmailList));
            set => base.SetProperty(nameof(EmailList), value);
        }

        [RepositoryProperty(nameof(MailSubject), RepositoryDataType.String)]
        public string MailSubject
        {
            get => base.GetProperty<string>(nameof(MailSubject));
            set => base.SetProperty(nameof(MailSubject), value);
        }

        [RepositoryProperty(nameof(EmailFrom), RepositoryDataType.String)]
        public string EmailFrom
        {
            get => base.GetProperty<string>(nameof(EmailFrom));
            set => base.SetProperty(nameof(EmailFrom), value);
        }

        [RepositoryProperty(nameof(EmailField), RepositoryDataType.String)]
        public string EmailField
        {
            get => base.GetProperty<string>(nameof(EmailField));
            set => base.SetProperty(nameof(EmailField), value);
        }

        [RepositoryProperty(nameof(AdminEmailTemplate), RepositoryDataType.Text)]
        public string AdminEmailTemplate
        {
            get => base.GetProperty<string>(nameof(AdminEmailTemplate));
            set => base.SetProperty(nameof(AdminEmailTemplate), value);
        }

        [RepositoryProperty(nameof(SubmitterEmailTemplate), RepositoryDataType.Text)]
        public string SubmitterEmailTemplate
        {
            get => base.GetProperty<string>(nameof(SubmitterEmailTemplate));
            set => base.SetProperty(nameof(SubmitterEmailTemplate), value);
        }

        [RepositoryProperty(nameof(OnlySingleResponse), RepositoryDataType.Int)]
        public bool OnlySingleResponse
        {
            get => base.GetProperty<int>(nameof(OnlySingleResponse)) != 0;
            set => base.SetProperty(nameof(OnlySingleResponse), value ? 1 : 0);
        }

        [RepositoryProperty(nameof(EnableNotificationMail), RepositoryDataType.Int)]
        public bool EnableNotificationMail
        {
            get => base.GetProperty<int>(nameof(EnableNotificationMail)) != 0;
            set => base.SetProperty(nameof(EnableNotificationMail), value ? 1 : 0);
        }

        // ================================================================================ Overrides

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case nameof(EmailList):
                    return EmailList;
                case nameof(MailSubject):
                    return MailSubject;
                case nameof(EmailFrom):
                    return EmailFrom;
                case nameof(EmailField):
                    return EmailField;
                case nameof(AdminEmailTemplate):
                    return AdminEmailTemplate;
                case nameof(SubmitterEmailTemplate):
                    return SubmitterEmailTemplate;
                case nameof(OnlySingleResponse):
                    return OnlySingleResponse;
                case nameof(EnableNotificationMail):
                    return EnableNotificationMail;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case nameof(EmailList):
                    EmailList = (string)value;
                    break;
                case nameof(MailSubject):
                    MailSubject = (string)value;
                    break;
                case nameof(EmailFrom):
                    EmailFrom = (string)value;
                    break;
                case nameof(EmailField):
                    EmailField = (string)value;
                    break;
                case nameof(AdminEmailTemplate):
                    AdminEmailTemplate = (string)value;
                    break;
                case nameof(SubmitterEmailTemplate):
                    SubmitterEmailTemplate = (string)value;
                    break;
                case nameof(OnlySingleResponse):
                    OnlySingleResponse = (bool)value;
                    break;
                case nameof(EnableNotificationMail):
                    EnableNotificationMail = (bool)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        // ================================================================================ Public API

        /// <summary>
        /// Checks whether the survey has already been filled at least one time by the current user.
        /// </summary>
        public bool IsFilled()
        {
            return IsFilled(User.Current as User);
        }
        /// <summary>
        /// Checks whether the survey has already been filled at least one time by the provided user.
        /// </summary>
        public bool IsFilled(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Content.All.DisableAutofilters().Any(c => c.InTree(this) && c.TypeIs("SurveyListItem") && (int)c["CreatedById"] == user.Id);
        }
        /// <summary>
        /// Checks whether the provided survey has already been filled at least one time by the current user.
        /// </summary>
        /// <param name="content">SurveyList content.</param>
        [ODataFunction]
        public static object IsFilled(Content content)
        {
            // For security reasons this OData function must not allow the client to
            // specifiy a different user other than the current one.

            if (content == null || !content.ContentType.IsInstaceOfOrDerivedFrom(typeof(SurveyList).Name))
                throw new InvalidOperationException("This action can only be called on a Survey.");

            return new
            {
                isFilled = ((SurveyList)content.ContentHandler).IsFilled()
            };
        }

        // ================================================================================ Helper methods

        internal string GetSenderEmail()
        {
            var sender = EmailFrom;
            return !string.IsNullOrEmpty(sender) ? sender : Configuration.Notification.DefaultEmailSender;
        }
    }
}
