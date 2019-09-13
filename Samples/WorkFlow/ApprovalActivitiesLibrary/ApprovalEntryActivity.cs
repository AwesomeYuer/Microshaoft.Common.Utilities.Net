namespace Microshaoft.WorkFlows.Activities
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Activities;

    public sealed class ApprovalActivity : AbstractJTokenWrapperIoActivity
    {
        public  JTokenWrapper OnExecuteProcess(NativeActivityContext context)
        {
            var inputs = Inputs.Get(context);
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
