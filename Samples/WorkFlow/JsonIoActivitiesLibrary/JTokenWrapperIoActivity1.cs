namespace Microshaoft.WorkFlows.Activities
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Activities;

    public sealed class JTokenWrapperIoActivity1 : AbstractJTokenWrapperIoActivity
    {
        public override JTokenWrapper OnExecuteProcess(NativeActivityContext context)
        {
            JToken parameter = Inputs.Get(context).Token;
            JArray steps = null;
            if (parameter["Steps"] == null)
            {
                steps = new JArray();
                parameter["Steps"] = steps;
            }
            else
            {
                steps = (JArray)parameter["Steps"];
            }
            steps
                .Add
                    (
                        $"Execute:{this.GetType().Name}@{DateTime.Now}"
                    );
            var r = new JTokenWrapper(parameter);
            return r;
        }

        public override JTokenWrapper OnResumeBookmarkProcess(NativeActivityContext context, Bookmark bookmark)
        {
            JToken parameter = Inputs.Get(context).Token;
            JArray steps = null;
            if (parameter["Steps"] == null)
            {
                steps = new JArray();
                parameter["Steps"] = steps;
            }
            else
            {
                steps = (JArray)parameter["Steps"];
            }
            steps
                .Add
                    (
                        $"Resume:{this.GetType().Name}@{DateTime.Now}"
                    );
            var r = new JTokenWrapper(parameter);
            return r;
        }
    }
}
