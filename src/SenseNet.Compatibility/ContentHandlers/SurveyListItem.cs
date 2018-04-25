using System;
using System.Linq;
using System.Net.Mail;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using System.Collections.Generic;
using System.IO;
using SenseNet.Configuration;
using Task = System.Threading.Tasks.Task;

namespace SenseNet.Portal.Portlets.ContentHandlers
{
    [ContentHandler]
    public class SurveyListItem : GenericContent
    {
        // ================================================================================ Properties

        protected List<Attachment> Attachments { get; set; }
        private List<Stream> Streams { get; set; }

        // ================================================================================ Constructors

        public SurveyListItem(Node parent) : this(parent, null) { }
        public SurveyListItem(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected SurveyListItem(NodeToken nt) : base(nt) { }

        // ================================================================================ Overrides

        public override void Save(NodeSaveSettings settings)
        {
            var isNew = IsNew;
            var import = RepositoryEnvironment.WorkingMode.Importing;
            if (isNew && !import)
                Name = GenerateName();

            var parentForm = LoadContentList() as SurveyList;
            if (isNew && !import && parentForm != null && parentForm.OnlySingleResponse)
            {
                // must not allow users to fill the form multiple times
                if (parentForm.IsFilled())
                    throw new InvalidOperationException(Compatibility.SR.GetString(
                        Compatibility.SR.Exceptions.Survey.OnlySingleResponse, User.Current.Username));
            }

            base.Save(settings);
            
            if (parentForm == null)
                return;

            if (isNew && !CopyInProgress && !import && parentForm.EnableNotificationMail)
            {
                // Sending notifications after filling a form is not part of the 
                // saving process so it can be performed asynchronously.
                Task.Run(() => SendMail());
            }
        }

        // ================================================================================ Notification

        protected virtual void SendMail()
        {
            if (!(LoadContentList() is SurveyList parentForm))
                return;

            var smtpClient = new SmtpClient();
            if (string.IsNullOrEmpty(smtpClient.Host))
                return;

            var itemContent = Content.Create(this);
            var fromEmail = parentForm.GetSenderEmail();
            var adminEmailList = string.Empty;

            if (!string.IsNullOrEmpty(parentForm.EmailList))
            {
                adminEmailList = parentForm.EmailList.Trim(' ', ';', ',')
                    .Replace(";", ",")
                    .Replace("\n", " ")
                    .Replace("\t", " ")
                    .Replace("\r", " ");
            }

            // send mail to administrator
            if (!string.IsNullOrEmpty(adminEmailList))
            {
                var ms = new MailMessage(fromEmail, adminEmailList)
                {
                    Subject = string.IsNullOrEmpty(parentForm.DisplayName)
                                               ? parentForm.Name
                                               : ReplaceField(parentForm.DisplayName, false, itemContent),
                    Body = string.IsNullOrEmpty(parentForm.AdminEmailTemplate)
                                            ? CreateEmailText()
                                            : ReplaceField(parentForm.AdminEmailTemplate, false, itemContent),
                    IsBodyHtml = !string.IsNullOrEmpty(parentForm.AdminEmailTemplate)
                };

                if (Attachments.Count > 0)
                {
                    foreach (var a in Attachments)
                        ms.Attachments.Add(a);
                }

                try
                {
                    smtpClient.Send(ms);
                }
                catch (Exception ex)
                {
                    SnLog.WriteException(ex,
                        $"Error during sending notification email to {ms.To} after a user completed the following form: {parentForm.Path}.");
                }
            }

            // send mail to submitter
            var submitterAddress = GetSubmitterAddress(itemContent, parentForm.EmailField);
            if (!string.IsNullOrEmpty(submitterAddress))
            {
                var ms = new MailMessage(fromEmail, submitterAddress)
                {
                    Subject = string.IsNullOrEmpty(parentForm.MailSubject)
                                               ? parentForm.Name
                                               : ReplaceField(parentForm.MailSubject, true, itemContent),
                    Body = string.IsNullOrEmpty(parentForm.SubmitterEmailTemplate)
                                            ? CreateEmailText()
                                            : ReplaceField(parentForm.SubmitterEmailTemplate, true, itemContent),
                    IsBodyHtml = !string.IsNullOrEmpty(parentForm.SubmitterEmailTemplate)
                };

                if (Attachments.Count > 0)
                {
                    foreach (var a in Attachments)
                        ms.Attachments.Add(a);
                }
                
                try
                {
                    smtpClient.Send(ms);
                }
                catch (Exception ex)
                {
                    SnLog.WriteException(ex,
                        $"Error during sending notification email to {ms.To} after completing the following form: {parentForm.Path}.");
                }
            }

            try
            {
                // dispose attachment streams
                if (Streams != null)
                {
                    foreach (var stream in Streams.Where(stream => stream != null))
                    {
                        stream.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                SnLog.WriteException(ex, $"Error when disposing attachments for {Path} (id: {Id}).");
            }
        }
        protected virtual string CreateEmailBody(bool isHtml)
        {
            var emailText = string.Empty;
            var content = Content.Create(this);
            var first = true;

            Attachments = new List<Attachment>();
            Streams = new List<Stream>();

            foreach (var field in content.Fields.Select(kvp => kvp.Value).Where(f => f.Name.StartsWith("#")))
            {
                if (field is BinaryField binaryField)
                {
                    if (binaryField.GetData() is BinaryData binaryData)
                    {
                        try
                        {
                            // Do not enclose this in a 'using' statement otherwise the Send 
                            // method will fail later because of a closed stream.
                            var stream = binaryData.GetStream();
                            if (stream != null)
                            {
                                // we will dispose these later
                                Streams.Add(stream);

                                var fileName = System.IO.Path.GetFileName(binaryData.FileName);
                                if (string.IsNullOrEmpty(fileName))
                                    fileName = $"{Name}-{field.Name.Replace("#", "-")}";

                                Attachments.Add(new Attachment(stream, fileName, binaryData.ContentType));
                            }
                        }
                        catch (Exception ex)
                        {
                            SnLog.WriteException(ex, $"Error when adding attachment to notification mail for {Path} (id: {Id}).");
                        }
                    }

                    continue;
                }

                if (first)
                    first = false;
                else
                    emailText = isHtml ? string.Concat(emailText, "<br/>") : string.Concat(emailText, "\n\r\n");

                emailText = string.Concat(emailText, field.DisplayName, ": ");
                emailText = field.FieldSetting.Bindings.Aggregate(emailText, (current, boundField) => string.Concat(current, Convert.ToString(content[boundField])));
            }

            return emailText;
        }
        protected virtual string CreateEmailText()
        {
            return $"\nName: {Name}\n\nUser: {CreatedBy.Name}\n\n\r\n{CreateEmailBody(false)}";
        }
        
        protected string ReplaceField(string body, bool isHtml, Content surveyItem)
        {
            try
            {
                body = body.Replace("{0}", CreateEmailBody(isHtml));

                var startIndex = body.IndexOf('{');

                while (startIndex >= 0)
                {
                    var cIdx = startIndex;
                    var endIdx = body.IndexOf('}', startIndex);

                    if (startIndex < endIdx)
                    {
                        cIdx = endIdx;

                        var fieldName = body.Substring(startIndex, endIdx - startIndex + 1);
                        fieldName = fieldName.Trim('{', '}');
                        if (surveyItem.Fields.ContainsKey(fieldName))
                        {
                            body = body.Remove(startIndex, endIdx - startIndex + 1);
                            var fieldData = surveyItem[fieldName];
                            var listValue = fieldData as List<string>;
                            var fieldValue = string.Empty;

                            if (listValue != null)
                            {
                                fieldValue = string.Join("; ", listValue);
                            }
                            else if (fieldData != null)
                            {
                                fieldValue = fieldData.ToString();
                            }

                            body = body.Insert(startIndex, fieldValue);
                            cIdx = startIndex + fieldValue.Length;
                        }
                    }
                    startIndex = body.IndexOf('{', cIdx);
                }

                return body;
            }
            catch (Exception ex)
            {
                SnLog.WriteException(ex, $"Error when replacing fields in notification email. List item: {Path}. Body: {body}");
                return body + " " + ex.Message;
            }
        }
        
        protected static string GetSubmitterAddress(Content content, string emailField)
        {
            if (string.IsNullOrEmpty(emailField) || !content.Fields.ContainsKey(emailField))
                return string.Empty;

            return content[emailField] as string;
        }

        // ================================================================================ Helper methods

        private string GenerateName()
        {
            #region from #23609

            return string.Concat(Parent.Name, "_", DateTime.UtcNow.ToString("yyyy_MM_dd___HH_mm_ss_fffffff"));

            #endregion
        }
    }
}
