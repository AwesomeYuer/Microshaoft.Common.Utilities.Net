namespace Microshaoft.WorkFlows.Activities
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Activities;

    public sealed class ApprovalExitActivity : AbstractWaitableActivity
    {
        public override bool ExecuteProcess(NativeActivityContext context)
        {
            return true;
        }

        public override bool OnResumeBookmarkProcess(NativeActivityContext context, Bookmark bookmark)
        {
            return true;
        }
    }
}
