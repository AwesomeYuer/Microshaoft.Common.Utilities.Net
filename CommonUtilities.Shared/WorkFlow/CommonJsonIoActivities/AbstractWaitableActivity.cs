#if NETFRAMEWORK4_X
namespace Microshaoft.WorkFlows.Activities
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Activities;
    using System.Diagnostics;

    public abstract class AbstractWaitableActivity : NativeActivity<JTokenWrapper>
    {
        [RequiredArgument]
        public InArgument<JTokenWrapper> Inputs { get; set; }

        [RequiredArgument]
        public InArgument<string> BookmarkName { get; set; }

        public abstract bool ExecuteProcess(NativeActivityContext context);
        

        protected override void Execute(NativeActivityContext context)
        {
            var inputs = Inputs.Get(context);
            var needWait = ExecuteProcess(context);
            if (needWait)
            {
                var bookmarkName = BookmarkName.Get(context);
                if (!bookmarkName.IsNullOrEmptyOrWhiteSpace())
                {
                    context
                        .CreateBookmark
                            (
                                bookmarkName
                                , new BookmarkCallback
                                    (
                                        (x, y, z) =>
                                        {
                                            var needExit = OnResumeBookmarkProcess(x, y);
                                            if (needExit)
                                            {
                                                Debug.Assert(Object.ReferenceEquals(context, x), $"{nameof(context)} ReferenceEquals {nameof(x)}");
                                                var parameter = Inputs.Get(x);
                                                Result
                                                .Set
                                                    (
                                                        context
                                                        , parameter
                                                    );
                                            }


                                        }
                                    )
                            );

                }
                else
                {
                    var parameter = inputs.TokenAs<JObject>();
                    parameter["ApprovalAction"] = "同意";
                    Result
                        .Set
                            (
                                context
                                , inputs
                            );

                }
                    
            }

            
                 
        }

        // NativeActivity derived activities that do asynchronous operations by calling 
        // one of the CreateBookmark overloads defined on System.Activities.NativeActivityContext 
        // must override the CanInduceIdle property and return true.
        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        public abstract bool OnResumeBookmarkProcess
                                    (
                                        NativeActivityContext context
                                        , Bookmark bookmark
                                      //  , object state
                                    );
    }
}
#endif