using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microshaoft;
using Newtonsoft.Json.Linq;

namespace WorkflowsHostFormsApplication
{
    public partial class Form1 : Form
    {
        private static string _xaml = File.OpenText("ApprovalStateMachineWorkFlow1.xaml").ReadToEnd();
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs ea)
        {
            JTokenWrapper dj = JTokenWrapper.Parse($@"{{""F1"":2222}}");
            var xx = ((JObject)dj.Token)["F1"].Value<int>();
            var inputs = new Dictionary<string, object>()
            {
                { "Inputs", dj}
            };
            var wfApp = WorkFlowHelper
                                .CreateApplication
                                    (
                                        "aa"
                                        , () =>
                                        {
                                            return
                                                _xaml;
                                        }
                                        , inputs
                                    );
            wfApp.Completed = (e) =>
            {
                //int Turns = Convert.ToInt32(e.Outputs["Turns"]);
                //Console.WriteLine("Congratulations, you guessed the number in {0} turns.", Turns);

                //syncEvent.Set();
                Console.WriteLine($"Completed {e.InstanceId}");
            };

            wfApp.Aborted = (e) =>
            {
                Console.WriteLine(e.Reason);
                //syncEvent.Set();
            };

            wfApp.OnUnhandledException = (e) =>
            {
                Console.WriteLine(e.UnhandledException.ToString());
                return UnhandledExceptionAction.Terminate;
            };

            wfApp.Idle = (e) =>
            {
                //idleEvent.Set();
            };

            wfApp.Run();

            


        }
    }
}
