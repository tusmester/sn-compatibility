using System;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.Diagnostics;
using System.Text;
using SenseNet.Configuration;

namespace SenseNet.Portal.Workspaces
{
    public class JournalObserver : NodeObserver
    {
        [Obsolete("After V6.5 PATCH 9: Use Journal.CreateJournalItems instead.")]
        public static bool CreateJournalItems => Journal.CreateJournalItems;

        protected override void OnNodeCreated(object sender, NodeEventArgs e)
        {
            // copy is logged separately
            if (!e.SourceNode.CopyInProgress)
                Log(e);
        }
        protected override void OnNodeModified(object sender, NodeEventArgs e)
        {
            Log(e);
        }
        protected override void OnNodeDeleted(object sender, NodeEventArgs e)
        {
            Log(e);
        }
        protected override void OnNodeDeletedPhysically(object sender, NodeEventArgs e)
        {
            Log(e);
        }

        protected override void OnNodeMoved(object sender, NodeOperationEventArgs e)
        {
            Log(e, e.OriginalSourcePath, e.TargetNode);
        }

        protected override void OnNodeCopied(object sender, NodeOperationEventArgs e)
        {
            Log(e, null, e.TargetNode);
        }

        private void Log(NodeEventArgs e)
        {
            Log(e, null, null);
        }

        private void Log(NodeEventArgs e, string sourcePath, Node target)
        {
            if (!Journal.CreateJournalItems)
                return;
            if (e.SourceNode == null)
                return;
            if (e.SourceNode.NodeOperation == NodeOperation.TemplateChildCopy || e.SourceNode.NodeOperation == NodeOperation.HiddenJournal)
                return;

            string path;
            try
            {
                path = e.SourceNode.Path;
            }
            catch (Exception ee) // logged
            {
                SnLog.WriteException(ee);
                path = "[error]";
            }

            var userName = "[nobody]";
            try
            {
                if (e.User != null)
                    userName = e.User.Name;
            }
            catch(Exception eee) // logged
            {
                SnLog.WriteException(eee);
                userName = "[error]";
            }
            string info = null;
            if (e.ChangedData != null)
            {
                var sb = new StringBuilder();
                foreach (var changedData in e.ChangedData)
                {
                    if (changedData.Name == "ModificationDate" ||
                        changedData.Name == "ModifiedById" ||
                        changedData.Name == "ModifiedBy" ||
                        changedData.Name == "VersionModificationDate" ||
                        changedData.Name == "VersionModifiedById" ||
                        changedData.Name == "VersionModifiedBy")
                        continue;

                    sb.Append(changedData.Name + ", ");
                }
                info = "{{$Wall:ChangedFields}}: " + sb.ToString().TrimEnd(',',' ');
            }
            
            var displayName = string.IsNullOrEmpty(e.SourceNode.DisplayName) ? e.SourceNode.Name : e.SourceNode.DisplayName;
            var targetPath = target?.Path;
            var targetDisplayName = target == null ? null : string.IsNullOrEmpty(target.DisplayName) ? target.Name : target.DisplayName;

            try
            {
                Journals.Add(e.EventType.ToString(), path, userName, e.Time, e.SourceNode.Id, displayName, e.SourceNode.NodeType.Name, sourcePath, targetPath, targetDisplayName, info, false);
            }
            catch (Exception ex)
            {
                SnLog.WriteException(ex, "Journal log failed.", EventId.Portal,
                    properties: new Dictionary<string, object>
                    {
                        {"Path", path},
                        {"Event type", e.EventType.ToString()},
                        {"Target path", targetPath}
                    });
            }
        }
    }
}
