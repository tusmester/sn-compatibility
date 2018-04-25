namespace SenseNet.ApplicationModel
{
    public class ShareAction : ClientAction
    {
        public override string MethodName
        {
            get => "SN.Wall.openShareDialog";
            set => base.MethodName = value;
        }

        public override string ParameterList
        {
            get => Content == null ? string.Empty : $@"'{Content.Id}'";
            set => base.ParameterList = value;
        }
    }
}
