#if NETFRAMEWORK4_X
namespace Microshaoft.WorkFlows.Activities
{
    using System;
    using System.Activities;

    public abstract class AbstractDynamicJsonIoActivity : NativeActivity<DynamicJson>
    {
        [RequiredArgument]
        public InArgument<DynamicJson> Inputs { get; set; }

        public abstract DynamicJson OnExecuteProcess(NativeActivityContext context);
        

        protected override void Execute(NativeActivityContext context)
        {
            DynamicJson parameter = Inputs.Get(context);
            if (parameter.IsArray)
            {

            }
            else if (parameter.IsObject)
            {
                var hasBookmark = parameter
                                    .IsDefined("bookmark");
                if (hasBookmark)
                {
                    var bookmarkName = parameter["bookmark"].GetValue<string>();
                    context
                        .CreateBookmark
                            (
                                bookmarkName
                                , new BookmarkCallback
                                    (
                                        (x, y, z) =>
                                        {
                                            var result = OnResumeBookmarkProcess(x, y);
                                            Result
                                                .Set
                                                    (
                                                        context
                                                        , result
                                                    );

                                        }
                                    )
                            );
                }
                else
                {
                    var result = OnExecuteProcess(context);
                    Result
                       .Set
                           (
                               context
                               , result
                           );
                }
            }
            else
            {
                throw
                    new ArgumentException
                            (
                                "Argument is not DynamicJson"
                                , "Inputs"
                            );
            }
        }

        // NativeActivity derived activities that do asynchronous operations by calling 
        // one of the CreateBookmark overloads defined on System.Activities.NativeActivityContext 
        // must override the CanInduceIdle property and return true.
        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        public abstract DynamicJson OnResumeBookmarkProcess
                                    (
                                        NativeActivityContext context
                                        , Bookmark bookmark
                                    );
    }
}
#endif