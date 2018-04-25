using System;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.Portal.Workspaces
{
    [ContentHandler]
    public class JournalNode : Node
    {
        private readonly JournalItem _journalItem;

        public JournalNode(Node parent, JournalItem journalItem) : base(parent, null) { _journalItem = journalItem; }
        protected JournalNode(NodeToken nt) : base(nt) { }

        public override bool IsContentType => false;

        public string What => _journalItem.What;
        public string Wherewith => _journalItem.Wherewith;
        public string Who => _journalItem.Who;
        public DateTime When => _journalItem.When;
    }
}
