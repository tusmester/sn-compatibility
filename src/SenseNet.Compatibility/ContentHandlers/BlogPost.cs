using System;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal
{
    [ContentHandler]
    public class BlogPost : GenericContent
    {
        public BlogPost(Node parent) : this(parent, null) { }
        public BlogPost(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected BlogPost(NodeToken nt) : base(nt) { }

        public const string PUBLISHEDON = "PublishedOn";
        [RepositoryProperty(nameof(PublishedOn), RepositoryDataType.DateTime)]
        public DateTime PublishedOn
        {
            get => GetProperty<DateTime>(nameof(PublishedOn));
            set => base.SetProperty(nameof(PublishedOn), value);
        }

        public override void Save(NodeSaveSettings settings)
        {
            base.Save(settings);
            MoveToFolder();
        }
        private void MoveToFolder()
        {
            if (DateTime.TryParse(this[nameof(PublishedOn)].ToString(), out var pubDate))
            {
                var dateFolderName = $"{pubDate.Year}-{pubDate.Month:00}";

                // check if the post is already in the proper folder
                if (ParentName == dateFolderName) return;

                // check if the proper folder exists
                var targetPath = RepositoryPath.Combine(WorkspacePath, String.Concat("Posts/", dateFolderName));
                if (!Exists(targetPath))
                {
                    // target folder needs to be created
                    Content.CreateNew("Folder", LoadNode(RepositoryPath.Combine(WorkspacePath, "Posts")), dateFolderName).Save();
                }

                // hide this move from journal
                NodeOperation = ContentRepository.Storage.NodeOperation.HiddenJournal;

                // move blog post to the proper folder
                MoveTo(LoadNode(targetPath));
            }
        }


        public override object GetProperty(string name)
        {
            switch (name)
            {
                case nameof(PublishedOn):
                    return PublishedOn;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case nameof(PublishedOn):
                    PublishedOn = (DateTime)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }
    }
}