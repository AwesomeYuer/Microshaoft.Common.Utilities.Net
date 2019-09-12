namespace Microshaoft.WorkFlows.Activities
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Activities;

    public sealed class ApprovalActivity : AbstractJTokenWrapperIoActivity
    {
        public  override JTokenWrapper OnExecuteProcess(NativeActivityContext context)
        {
            var inputs = Inputs.Get(context);
            var jObject = inputs.TokenAs<JObject>();
            //jObject["ApprovalAction"] = "同意";
            return
                inputs;
        }

        public override JTokenWrapper OnResumeBookmarkProcess(NativeActivityContext context, Bookmark bookmark)
        {
            var inputs = Inputs.Get(context);
            return
                inputs;
        }
    }
}
