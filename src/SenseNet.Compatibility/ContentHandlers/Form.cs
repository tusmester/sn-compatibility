using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.Portlets.ContentHandlers
{
	[ContentHandler]
	public class Form : ContentList
	{
        public Form(Node parent) : this(parent, null) { }
		public Form(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected Form(NodeToken nt) : base(nt)	{ }

		[RepositoryProperty("EmailList", RepositoryDataType.Text)]
		public string EmailList
		{
			get => GetProperty<string>("EmailList");
		    set => this["EmailList"] = value;
		}

        [RepositoryProperty("TitleSubmitter", RepositoryDataType.String)]
        public string TitleSubmitter
        {
            get => GetProperty<string>("TitleSubmitter");
            set => this["TitleSubmitter"] = value;
        }

		[RepositoryProperty("AfterSubmitText", RepositoryDataType.Text)]
		public string AfterSubmitText
		{
			get => GetProperty<string>("AfterSubmitText");
		    set => this["AfterSubmitText"] = value;
		}

        [RepositoryProperty("EmailFrom", RepositoryDataType.String)]
        public string EmailFrom
        {
			get => GetProperty<string>("EmailFrom");
            set => this["EmailFrom"] = value;
        }

        [RepositoryProperty("EmailFromSubmitter", RepositoryDataType.String)]
        public string EmailFromSubmitter
        {
            get => GetProperty<string>("EmailFromSubmitter");
            set => this["EmailFromSubmitter"] = value;
        }

        [RepositoryProperty("EmailField", RepositoryDataType.String)]
        public string EmailField
        {
			get => GetProperty<string>("EmailField");
            set => this["EmailField"] = value;
        }

        [RepositoryProperty("EmailTemplate", RepositoryDataType.Text)]
        public string EmailTemplate
        {
			get => GetProperty<string>("EmailTemplate");
            set => this["EmailTemplate"] = value;
        }

        [RepositoryProperty("EmailTemplateSubmitter", RepositoryDataType.Text)]
        public string EmailTemplateSubmitter
        {
            get => GetProperty<string>("EmailTemplateSubmitter");
            set => this["EmailTemplateSubmitter"] = value;
        }
	}
}