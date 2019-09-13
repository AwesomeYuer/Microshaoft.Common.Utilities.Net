namespace WorkflowConsoleApplication
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Activities;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    class Program
    {
        private static string _xaml = File.OpenText("ApprovalStateMachineWorkFlow1.xaml").ReadToEnd();

        static void Main()
        {
            JTokenWrapperIoWorkFlowTest(1);
            Console.ReadLine();
            return;
            System.Threading.Tasks.Parallel
                    .For
                        (
                            1
                            , 2
                            , new ParallelOptions()
                            {
                                MaxDegreeOfParallelism = Environment.ProcessorCount
                            }
                            , (x) =>
                            {

                                //try
                                //{
                                JTokenWrapperIoWorkFlowTest(x);
                                //DynamicJsonIoWorkFlowTest(x);
                                //}
                                //catch (Exception e)
                                //{

                                //    Console.WriteLine(e);
                                //}

                            }
                        );
            Console.ReadLine();
        }

        private static void JTokenWrapperIoWorkFlowTest(int i)
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

        private static void DynamicJsonIoWorkFlowTest(int i)
        {
            DynamicJson dj = DynamicJson.Parse($@"{{""F1"":{i}}}");
            var xx = dj["F1"].GetValue<int>();
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

        static void Main1111(string[] args)
        {
            //ProcessOnce(1);
            //Console.ReadLine();
            //return;
            System.Threading.Tasks.Parallel
                    .For
                        (
                            1
                            , 1000
                            , new ParallelOptions()
                            {
                                MaxDegreeOfParallelism = Environment.ProcessorCount
                            }
                            , (x) =>
                            {

                                //try
                                //{
                                    ProcessOnce(x);
                                //}
                                //catch (Exception e)
                                //{

                                //    Console.WriteLine(e);
                                //}
                               
                            }
                        );

            Console.ReadLine();
        }
        static void ProcessOnce(int i)
        {
            var wfApp = WorkFlowHelper
                            .CreateApplication
                                (
                                    "a:" + (i % 4).ToString()
                                    , () =>
                                    {
                                        return
                                            _xaml;
                                    }
                                );

            wfApp.Completed = (e) =>
            {
                //Console.WriteLine(e.InstanceId);
            };

            wfApp.Aborted = (e) =>
            {
              
            };

            //wfApp.OnUnhandledException = (e) =>
            //{
            //    Console.WriteLine(e.UnhandledException.ToString());
            //    return UnhandledExceptionAction.Terminate;
            //};

            //wfApp.Idle = (e) =>
            //{
            //    idleEvent.Set();
            //};


            var config = @"{
                                ""WorkflowInstanceQuery"" :
                                                            [{
                                                                ""States"":
                                                                            [
                                                                                ""*""
                                                                            ]
                                                                , ""QueryAnnotations"": {}
                                                            }]
                               , ""ActivityStateQuery"" :
                                                            [{
                                                                ""ActivityName"": ""*""
                                                                , ""Arguments"": []
                                                                , ""Variables"": []
                                                                , ""States"": [""*""]
                                                                , ""QueryAnnotations"": {}
                                                            }]
                                ,
                                ""CustomTrackingQuery"": [{
                                                                ""Name"": ""*"",
                                                                ""ActivityName"": ""*"",
                                                                ""QueryAnnotations"": {}
                                                            }]
                                ,
                                ""FaultPropagationQuery"": [{
                                                                ""FaultHandlerActivityName"": ""*"",
                                                                ""FaultSourceActivityName"": ""*"",
                                                                ""QueryAnnotations"": {}
                                                                }],
                                ""BookmarkResumptionQuery"": [{
                                                                    ""Name"": ""*"",
                                                                    ""QueryAnnotations"": {}
                                                                    }],
                                ""ActivityScheduledQuery"": [{
                                                                ""ActivityName"": ""*"",
                                                                ""ChildActivityName"": ""*"",
                                                                ""QueryAnnotations"": {}
                                                                }],
                                ""CancelRequestedQuery"": [{
                                                                ""ActivityName"": ""*"",
                                                                ""ChildActivityName"": ""*"",
                                                                ""QueryAnnotations"": {}
                                                                }]
                            }";
            var trackingProfile = WorkFlowHelper
                                        .GetTrackingProfileFromJson
                                                (
                                                    config
                                                    , true
                                                );
            var etwTrackingParticipant = new EtwTrackingParticipant();
            etwTrackingParticipant.TrackingProfile = trackingProfile;
            var commonTrackingParticipant = new CommonTrackingParticipant()
            {
                TrackingProfile = trackingProfile
                ,
                OnTrackingRecordReceived = (x, y) =>
                {
                    //Console.WriteLine("{1}{0}{2}", ",", x, y);
                    return true;
                }
            };

            wfApp
                .Extensions
                .Add
                    (
                        etwTrackingParticipant
                    );
            wfApp
                .Extensions
                .Add
                    (
                        commonTrackingParticipant
                    );

            wfApp.Run();

            // Loop until the workflow completes.
            //WaitHandle[] handles = new WaitHandle[] { syncEvent, idleEvent };
            //while (WaitHandle.WaitAny(handles) != 0)
            //{
            //    // Gather the user input and resume the bookmark.
            //    bool validEntry = false;
            //    while (!validEntry)
            //    {
            //        int Guess;
            //        if (!int.TryParse(Console.ReadLine(), out Guess))
            //        {
            //            Console.WriteLine("Please enter an integer.");
            //        }
            //        else
            //        {
            //            validEntry = true;
            //            wfApp.ResumeBookmark("EnterGuess", Guess);
            //        }
            //    }
            //}
           
        }
    }
}
