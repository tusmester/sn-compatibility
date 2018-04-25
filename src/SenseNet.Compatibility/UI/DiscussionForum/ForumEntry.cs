using System.Collections.Generic;
using System.Linq;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Search;
using System.Diagnostics;
using SenseNet.ContentRepository.Search;

namespace SenseNet.Portal.DiscussionForum
{
    [ContentHandler]
    public class ForumEntry : GenericContent
    {
        public ForumEntry(Node parent) : this(parent, null) { }
		public ForumEntry(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected ForumEntry(NodeToken nt) : base(nt) { }

        [RepositoryProperty("ReplyTo", RepositoryDataType.Reference)]
        public virtual Node ReplyTo
        {
            get => GetReference<Node>("ReplyTo");
            set => SetReference("ReplyTo", value);
        }

        [RepositoryProperty("PostedBy", RepositoryDataType.Reference)]
        public virtual Node PostedBy
        {
            get => GetReference<Node>("PostedBy");
            set => SetReference("PostedBy", value);
        }

        [RepositoryProperty("SerialNo")]
        public virtual int SerialNo
        {
            get => (int)base.GetProperty("SerialNo");
            set => this["SerialNo"] = value;
        }

        private int? _replyToNo;
        public virtual int ReplyToNo
        {
            get
            {
                if (_replyToNo == null)
                {
                    if (ReplyTo != null)
                        _replyToNo = ((ForumEntry)ReplyTo).SerialNo;
                }

                return _replyToNo ?? -1;
            }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "ReplyTo":
                    return ReplyTo;
                case "PostedBy":
                    return PostedBy;
                case "ReplyToNo":
                    return ReplyToNo;
                case "SerialNo":
                    return SerialNo;
                default:
                    return base.GetProperty(name);
            }
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "ReplyTo":
                    ReplyTo = (Node)value;
                    break;
                case "PostedBy":
                    PostedBy = (Node)value;
                    break;
                case "SerialNo":
                    SerialNo = (int)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        protected override void OnCreating(object sender, ContentRepository.Storage.Events.CancellableNodeEventArgs e)
        {
            base.OnCreating(sender, e);

            PostedBy = CreatedBy;
            if (!SearchManager.ContentQueryIsAllowed)
                return;

            try
            {
                var q = ContentQuery.Query("+InTree:@0 +Type:'ForumEntry' +CreationDate:<@1 .COUNTONLY", 
                    null, ParentPath, CreationDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                SerialNo = q.Count;
            }
            catch
            {
                Trace.Write("Lucene query failed on node " + Path);
            }
        }

        public IEnumerable<ForumEntry> GetReplies()
        {
            return ContentQuery.Query($"+InFolder:{ParentPath} +ReplyTo:{Id}").Nodes.OfType<ForumEntry>();
        }
    }
}
