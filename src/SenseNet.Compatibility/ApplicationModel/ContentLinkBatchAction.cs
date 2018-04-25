namespace SenseNet.ApplicationModel
{
    public class ContentLinkBatchAction : OpenPickerAction
    {
        protected override string TargetActionName => "ContentLinker";

        protected override string TargetParameterName => "ids";

        protected override string GetIdList()
        {
            return GetIdListMethod();
        }
    }
}
