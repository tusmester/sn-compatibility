using System;
using System.Linq;
using System.Net.Mail;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.i18n;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using System.Web;
using SenseNet.ContentRepository.Search;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.Search;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.Portal.Portlets.ContentHandlers
{
    [ContentHandler]
    public class EventRegistrationFormItem : FormItem
    {
        private bool _admin;
        private string _eventTitle;

        // ================================================================================= Properties

        public string EventTitle
        {
            get
            {
                if (string.IsNullOrEmpty(_eventTitle))
                {
                    
                    if (ContentQuery.Query(Compatibility.SafeQueries.CalendarEventsForRegistration,
                            QuerySettings.AdminSettings, ParentId).Nodes.FirstOrDefault() is GenericContent eventContent)
                    {
                        _eventTitle = eventContent.Content.DisplayName;
                    }
                }
                
                return _eventTitle;
            }
        }

        // ================================================================================= Constructors

        public EventRegistrationFormItem(Node parent) : this(parent, null) { }
        public EventRegistrationFormItem(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected EventRegistrationFormItem(NodeToken nt) : base(nt) { }

        // ================================================================================= Methods

        public override bool IsTrashable => false;

        protected override string CreateEmailBody(bool isHtml)
        {
            var emailText = "";
            var c = Content.Create(this);
            var first = true;
            foreach (var kvp in c.Fields)
            {
                var f = kvp.Value;

                if (!f.Name.StartsWith("#") || f.Name == "Email")
                    continue;

                if (first)
                    first = false;
                else
                    emailText = isHtml ? string.Concat(emailText, "<br/>") : string.Concat(emailText, "\n\r\n");

                emailText = string.Concat(emailText, f.DisplayName, ": ");
                foreach (var b in f.FieldSetting.Bindings)
                    emailText = string.Concat(emailText, Convert.ToString(this[b]));
            }
            emailText = isHtml ? string.Concat(emailText, "<br/>") : string.Concat(emailText, "\n\r\n");
            emailText = string.Concat(emailText, SenseNetResourceManager.Current.GetString("EventRegistration", "ToCancel"));
            emailText = isHtml ? string.Concat(emailText, "<br/>") : string.Concat(emailText, "\n\r\n");
            emailText = isHtml
                             ? string.Concat(emailText, "<a href=", GenerateCancelLink(), @""">", GenerateCancelLink(), "</a>")
                             : string.Concat(emailText, GenerateCancelLink());

            if (_admin)
            {
                emailText = isHtml ? string.Concat(emailText, "<br/>") : string.Concat(emailText, "\n\r\n");
                emailText = string.Concat(emailText, SenseNetResourceManager.Current.GetString("EventRegistration", "ToApprove"));
                emailText = isHtml ? string.Concat(emailText, "<br/>") : string.Concat(emailText, "\n\r\n");
                emailText = isHtml
                                 ? string.Concat(emailText, "<a href=", GenerateApproveLink(), @""">", GenerateApproveLink(), "</a>")
                                 : string.Concat(emailText, GenerateApproveLink());
            }

            return emailText;
        }

        protected override string CreateEmailText()
        {
            var emailText = string.Empty;
            emailText = string.Concat(emailText, "\nName: ", Name, "\n");
            emailText = string.Concat(emailText, "\nUser: ", CreatedBy.Name, "\n");
            emailText = string.Concat(emailText, "\n\r\n", CreateEmailBody(false));
            return emailText;
        }

        private string CreateAdminemailText()
        {
            var emailText = CreateEmailText();
            return emailText;
        }

        private string GenerateCancelLink()
        {
            var page = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            return page + Path + "?action=Cancel&back=" + page;
        }

        private string GenerateApproveLink()
        {
            var page = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            return page + Path + "?action=Approve&back=" + page;
        }

        public void SendCancellationMail()
        {
            if (LoadContentList() is Form parentForm)
            {
                var itemContent = Content.Create(this);
                var sc = new SmtpClient();
                if (string.IsNullOrEmpty(sc.Host))
                    return;

                var from = GetSender(parentForm);
                var emailList = string.Empty;
                if (!string.IsNullOrEmpty(parentForm.EmailList))
                {
                    emailList = parentForm.EmailList.Trim(' ', ';', ',')
                        .Replace(";", ",").Replace("\n", " ").Replace("\t", " ").Replace("\r", " ");
                }

                // send mail to administrator
                _admin = true;
                if (!string.IsNullOrEmpty(emailList))
                {
                    var ms = new MailMessage(@from, emailList)
                    {
                        Subject = string.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "AdminCancellingSubject"),
                            string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                ? EventTitle
                                : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                        Body = "Event registration cancelled."
                    };

                    sc.Send(ms);
                }


                // ============= Send notification email
                // send mail to submitter
                _admin = false;
                var submitterAddress = GetSubmitterAddress(parentForm.EmailField, itemContent);
                if (!string.IsNullOrEmpty(submitterAddress))
                {
                    var fromField = GetSenderOfSubmiter(parentForm);
                    // send mail to submitter
                    var ms = new MailMessage(fromField, submitterAddress)
                    {
                        Subject = string.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "UserCancellingSubject"),
                            string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                ? EventTitle
                                : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                        Body = "Event registration cancelled",
                        IsBodyHtml = !string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                    };

                    sc.Send(ms);
                }
            }
        }

        protected override void SendMail()
        {
            if (LoadContentList() is Form parentForm)
            {
                var itemContent = Content.Create(this);
                var sc = new SmtpClient();
                if (string.IsNullOrEmpty(sc.Host))
                    return;

                var from = GetSender(parentForm);
                var emailList = string.Empty;
                if (!string.IsNullOrEmpty(parentForm.EmailList))
                {
                    emailList = parentForm.EmailList.Trim(' ', ';', ',')
                        .Replace(";", ",").Replace("\n", " ").Replace("\t", " ").Replace("\r", " ");
                }

                // send mail to administrator
                _admin = true;
                if (!string.IsNullOrEmpty(emailList))
                {
                    var ms = new MailMessage(@from, emailList)
                    {
                        Subject = string.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "AdminSubscriptionSubject"),
                            string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                ? EventTitle
                                : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                        Body = string.IsNullOrEmpty(parentForm.EmailTemplate)
                            ? CreateAdminemailText()
                            : ReplaceField(parentForm.EmailTemplate, false, itemContent)
                    };

                    sc.Send(ms);
                }


                // ============= Send notification email
                // send mail to submitter
                _admin = false;
                var submitterAddress = GetSubmitterAddress(parentForm.EmailField, itemContent);
                if (!string.IsNullOrEmpty(submitterAddress))
                {
                    var fromField = GetSenderOfSubmiter(parentForm);

                    // send mail to submitter
                    var ms = new MailMessage(fromField, submitterAddress)
                    {
                        Subject = string.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "UserSubscriptionSubject"),
                            string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                ? EventTitle
                                : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                        Body = string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                            ? CreateEmailText()
                            : ReplaceField(parentForm.EmailTemplateSubmitter, true, itemContent),
                        IsBodyHtml = !string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                    };

                    sc.Send(ms);
                }
            }
        }

        private void CheckParticipantNumber(bool isApproving)
        {
            using (new SystemAccount())
            {
                var form = Content.Load(ParentId);

                var subs = form.Children;

                var overallWantedGuests = 1;

                // Collecting approved subscriptions and getting participants.
                foreach (var node in subs)
                {
                    if (node["Version"].ToString() == "V1.0.A")
                    {
                        int.TryParse(node["GuestNumber"].ToString(), out var guests);
                        overallWantedGuests += 1 + guests;
                    }
                }

                int.TryParse(this["GuestNumber"].ToString(), out var thisGuests);

                overallWantedGuests += thisGuests;

                var result = ContentQuery.Query(Compatibility.SafeQueries.CalendarEventsForRegistration,
                    new QuerySettings {EnableAutofilters = FilterStatus.Disabled}, ParentId);
                if (result.Count == 0)
                    throw new NotSupportedException("This registration form must be connected to an event before you can register");
                if (result.Count > 1)
                    throw new NotSupportedException("This registration form is connected to more than one event");

                var eventContent = Content.Create(result.Nodes.First());

                int.TryParse(eventContent["MaxParticipants"].ToString(), out var maxPart);

                if (overallWantedGuests > maxPart)
                {
                    if (!isApproving)
                    {
                        throw new Exception(string.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "TooManyGuests"), maxPart - (overallWantedGuests - thisGuests)));
                    }

                    throw new Exception(string.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "TooManyParticipants"), maxPart - (overallWantedGuests - thisGuests - 1)));
                }
            }
        }

        public override void Save(SavingMode mode)
        {
            // skip this check when content query is not available (e.g. importing)
            if (SearchManager.ContentQueryIsAllowed)
                CheckParticipantNumber(false);
            
            base.Save(mode);
        }

        public override void Approve()
        {
            CheckParticipantNumber(true);
            base.Approve();

            if (LoadContentList() is Form parentForm)
            {
                var itemContent = Content.Create(this);
                var sc = new SmtpClient();
                if (string.IsNullOrEmpty(sc.Host))
                    return;

                var from = GetSender(parentForm);
                var emailList = string.Empty;
                if (!string.IsNullOrEmpty(parentForm.EmailList))
                {
                    emailList = parentForm.EmailList.Trim(' ', ';', ',')
                        .Replace(";", ",").Replace("\n", " ").Replace("\t", " ").Replace("\r", " ");
                }

                // send mail to administrator
                _admin = true;
                if (!string.IsNullOrEmpty(emailList))
                {
                    var ms = new MailMessage(@from, emailList)
                    {
                        Subject = string.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "AdminApprovingSubject"),
                            string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                ? EventTitle
                                : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                        Body = string.IsNullOrEmpty(parentForm.EmailTemplate)
                            ? CreateAdminemailText()
                            : SenseNetResourceManager.Current.GetString("EventRegistration", "AdminApprovingBody")
                    };

                    sc.Send(ms);
                }

                // send mail to submitter
                _admin = false;
                var submitterAddress = GetSubmitterAddress(parentForm.EmailField, itemContent);
                if (!string.IsNullOrEmpty(submitterAddress))
                {
                    var fromField = GetSenderOfSubmiter(parentForm);
                    // send mail to submitter
                    var ms = new MailMessage(fromField, submitterAddress)
                    {
                        Subject = string.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "UserApprovingSubject"),
                            string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                ? EventTitle
                                : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                        Body = string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                            ? CreateEmailText()
                            : SenseNetResourceManager.Current.GetString("EventRegistration", "UserApprovingBody"),
                        IsBodyHtml = !string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                    };

                    sc.Send(ms);

                }
            }
        }

        public override void Reject()
        {
            base.Reject();

            if (LoadContentList() is Form parentForm)
            {
                var itemContent = Content.Create(this);
                var sc = new SmtpClient();
                if (string.IsNullOrEmpty(sc.Host))
                    return;

                var from = GetSender(parentForm);
                var emailList = string.Empty;
                if (!string.IsNullOrEmpty(parentForm.EmailList))
                {
                    emailList = parentForm.EmailList.Trim(' ', ';', ',')
                        .Replace(";", ",").Replace("\n", " ").Replace("\t", " ").Replace("\r", " ");
                }

                // send mail to administrator
                _admin = true;
                if (!string.IsNullOrEmpty(emailList))
                {
                    var ms = new MailMessage(@from, emailList)
                    {
                        Subject = string.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "AdminRejectingSubject"),
                            string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                ? EventTitle
                                : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                        Body = string.IsNullOrEmpty(parentForm.EmailTemplate)
                            ? CreateAdminemailText()
                            : SenseNetResourceManager.Current.GetString("EventRegistration", "AdminRejectingBody")
                    };

                    sc.Send(ms);
                }

                // send mail to submitter
                _admin = false;
                var submitterAddress = GetSubmitterAddress(parentForm.EmailField, itemContent);
                if (!string.IsNullOrEmpty(submitterAddress))
                {
                    var fromField = GetSenderOfSubmiter(parentForm);
                    // send mail to submitter
                    var ms = new MailMessage(fromField, submitterAddress)
                    {
                        Subject = string.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "UserRejectingSubject"),
                            string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                ? EventTitle
                                : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                        Body = string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                            ? CreateEmailText()
                            : SenseNetResourceManager.Current.GetString("EventRegistration", "UserRejectingBody"),
                        IsBodyHtml = !string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                    };

                    sc.Send(ms);
                }
            }
        }

        protected override void OnDeletedPhysically(object sender, NodeEventArgs e)
        {
            base.OnDeletedPhysically(sender, e);

            SendCancellationMail();
        }
    }
}