namespace NumberGuessWorkflowHost
{
    using Microshaoft;
    using NumberGuessWorkflowActivities;
    using System;
    using System.Activities;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            AutoResetEvent syncEvent = new AutoResetEvent(false);
            AutoResetEvent idleEvent = new AutoResetEvent(false);

            var inputs = new Dictionary<string, object>() { { "MaxNumber", 100 } };

            WorkflowApplication wfApp =
                new WorkflowApplication(new SequentialNumberGuessWorkflow(), inputs);

            wfApp.Completed = (e) =>
            {
                int Turns = Convert.ToInt32(e.Outputs["Turns"]);
                Console.WriteLine("Congratulations, you guessed the number in {0} turns.", Turns);

                syncEvent.Set();
            };

            wfApp.Aborted = (e) =>
            {
                Console.WriteLine(e.Reason);
                syncEvent.Set();
            };

            wfApp.OnUnhandledException = (e) =>
            {
                Console.WriteLine(e.UnhandledException.ToString());
                return UnhandledExceptionAction.Terminate;
            };

            wfApp.Idle = (e) =>
            {
                idleEvent.Set();
            };


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
                                              Console.WriteLine("{1}{0}{2}", ",", x, y);
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
            WaitHandle[] handles = new WaitHandle[] { syncEvent, idleEvent };
            while (WaitHandle.WaitAny(handles) != 0)
            {
                // Gather the user input and resume the bookmark.
                bool validEntry = false;
                while (!validEntry)
                {
                    int Guess;
                    if (!int.TryParse(Console.ReadLine(), out Guess))
                    {
                        Console.WriteLine("Please enter an integer.");
                    }
                    else
                    {
                        validEntry = true;
                        wfApp.ResumeBookmark("EnterGuess", Guess);
                    }
                }
            }
        }
    }
}
