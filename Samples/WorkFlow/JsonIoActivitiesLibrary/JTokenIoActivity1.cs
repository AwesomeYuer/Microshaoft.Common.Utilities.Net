namespace Microshaoft.WorkFlows.Activities
{
    using Newtonsoft.Json.Linq;
    using System.Activities;

    public sealed class JTokenIoActivity1 : AbstractJTokenIoActivity
    {
        public override JToken ExecuteProcess(NativeActivityContext context)
        {
            JToken parameter = Inputs.Get(context);
            //JArray steps = null; 
            //if (parameter["Steps"] == null)
            //{
            //    steps = new JArray();
            //    parameter["Steps"] = steps;
            //}
            //else
            //{
            //    steps = (JArray)parameter["Steps"];
            //}
            //steps
            //    .Add
            //        (
            //            $"Execute:{this.GetType().Name}@{DateTime.Now}"
            //        );
            return parameter;
        }

        public override JToken OnResumeBookmarkProcess(NativeActivityContext context, Bookmark bookmark)
        {
            JToken parameter = Inputs.Get(context);
            //JArray steps = null;
            //if (parameter["Steps"] == null)
            //{
            //    steps = new JArray();
            //    parameter["Steps"] = steps;
            //}
            //else
            //{
            //    steps = (JArray)parameter["Steps"];
            //}
            //steps
            //    .Add
            //        (
            //            $"Resume:{this.GetType().Name}@{DateTime.Now}"
            //        );
            return parameter;
        }
    }
}
