#if NETFRAMEWORK4_X
namespace Microshaoft.WorkFlows.Activities
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Activities;

    public abstract class AbstractJTokenWrapperIoActivity : NativeActivity<JTokenWrapper>
    {
        [RequiredArgument]
        public InArgument<JTokenWrapper> Inputs { get; set; }

        public abstract JTokenWrapper ExecuteProcess(NativeActivityContext context);
        

        protected override void Execute(NativeActivityContext context)
        {
            JToken parameter = Inputs.Get(context).Token;
            if (parameter is JArray)
            {

            }
            else if (parameter is JObject)
            {
                ((JObject)parameter)["F2"] = "aaaaaaaaaaaaaa";


                var bookmark = parameter["bookmark"];
                var jObject = (JObject)parameter;
                var hasBookmark = jObject
                                    .TryGetValue
                                        (
                                            "bookmark"
                                            , StringComparison.OrdinalIgnoreCase
                                            , out var o
                                        );
                if (hasBookmark)
                {
                    var bookmarkName = o.Value<string>();
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
                    var result = ExecuteProcess(context);
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
                                "Argument is not JToken"
                                , "Inputs"
                            );
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

        public abstract JTokenWrapper OnResumeBookmarkProcess
                                    (
                                        NativeActivityContext context
                                        , Bookmark bookmark
                                      //  , object state
                                    );
    }
}
#endif