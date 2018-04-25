using System.Collections.Generic;
using System.Linq;
using SenseNet.ContentRepository;

namespace SenseNet.ApplicationModel
{
    [Scenario("SurveySettings")]
    internal class SurveySettingsScenario : GenericScenario
    {
        protected override IEnumerable<ActionBase> CollectActions(Content context, string backUrl)
        {
            return base.CollectActions(context, backUrl).ToList();
        }
    }
}
