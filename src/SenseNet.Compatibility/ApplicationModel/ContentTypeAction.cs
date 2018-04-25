using SenseNet.Diagnostics;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.ApplicationModel
{
    public class ContentTypeAction : UrlAction
    {
        public override string Uri
        {
            get
            {
                // In case of a missing content or dynamically generated
                // content type we cannot do anything.
                if (Content == null || Forbidden || Content.ContentType.IsNew)
                    return string.Empty;

                try
                {
                    if (!SecurityHandler.HasPermission(Content.ContentType, PermissionType.Save))
                        return string.Empty;

                    var s = SerializeParameters(GetParameteres());

                    // we provide the edit action of the CTD here
                    var uri = $"{Content.ContentType.Path}?action=Edit{s}";

                    if (IncludeBackUrl && !string.IsNullOrEmpty(BackUri))
                    {
                        uri += $"&{PortalContext.BackUrlParamName}={System.Uri.EscapeDataString(BackUri)}";
                    }

                    return uri;
                }
                catch (SenseNetSecurityException ex)
                {
                    SnLog.WriteException(ex);
                }

                return string.Empty;
            }
        }
    }
}