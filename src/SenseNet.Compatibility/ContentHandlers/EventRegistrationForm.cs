using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.Portlets.ContentHandlers
{
    [ContentHandler]
    public class EventRegistrationForm : Form
    {
        // ================================================================================= Constructors

        public EventRegistrationForm(Node parent) : this(parent, null) { }
        public EventRegistrationForm(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected EventRegistrationForm(NodeToken nt) : base(nt) { }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case nameof(Event):
                    return Event;
                default:
                    return base.GetProperty(name);
            }
        }


        [RepositoryProperty(nameof(Event), RepositoryDataType.Reference)]
        public Node Event
        {
            get => GetReference<Node>(nameof(Event));
            set => SetReference(nameof(Event), value);
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case nameof(Event):
                    Event = (Node)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }
    }
}
