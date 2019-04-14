using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Newtonsoft.Json.Linq;
namespace Microshaoft.WorkFlows.Activities
{

    public sealed class DynamicJsonIoActivity1 : AbstractDynamicJsonIoActivity
    {
        public override DynamicJson OnExecuteProcess(NativeActivityContext context)
        {
            dynamic parameter = Inputs.Get(context);
            var xx = parameter["F1"].GetValue<int>();
            Console.WriteLine($"Inputs F1:{xx} @ {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff")}");
            var i = -1;
            if (!parameter.IsDefined("RunningCount"))
            {
                i = parameter.RunningCount = 0;
            }
            else
            {
                i = parameter.RunningCount += 1;
            }
            if (!parameter.IsDefined("Arr"))
            {
                parameter.Arr = new[] { new { Text = $"run{i}" } };
            }
            else
            {
                parameter.Arr.Add(new { Text = $"run{i}" });
            }
            parameter.Arr[i].F2 = $"F22222222222@{i}";
            Console.WriteLine(parameter.Arr.Count());

            return parameter;
        }

        public override DynamicJson OnResumeBookmarkProcess(NativeActivityContext context, Bookmark bookmark)
        {
            DynamicJson parameter = Inputs.Get(context);
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
