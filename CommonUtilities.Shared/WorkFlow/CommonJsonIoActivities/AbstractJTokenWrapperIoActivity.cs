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


        [RequiredArgument]
        public InArgument<bool> AutoSetBookmark { get; set; }

        public abstract JTokenWrapper OnExecuteProcess(NativeActivityContext context);


        protected override void Execute(NativeActivityContext context)
        {
            var inputs = Inputs.Get(context);
            JObject jObject = inputs.TokenAs<JObject>();

            var result = OnExecuteProcess(context);
            var autoSetBookmark = AutoSetBookmark.Get(context);
            if (!autoSetBookmark)
            {
                return;
            }




            var bookmarkName = string.Empty;

            var hasBookmark = jObject
                        .TryGetValue
                            (
                                "bookmarkName"
                                , StringComparison.OrdinalIgnoreCase
                                , out var j
                            );
            if (hasBookmark)
            {
                bookmarkName = j.Value<string>();
            }
            hasBookmark = !bookmarkName.IsNullOrEmptyOrWhiteSpace();
            if (!hasBookmark)
            {
                bookmarkName = "aaaaaaaaaa";
                jObject["BookmarkName"] = bookmarkName;
                context
                        .CreateBookmark
                            (
                                bookmarkName
                                , new BookmarkCallback
                                    (
                                        OnBookmarkCallback
                                    )
                            );
                
            }
            //else
            //{
            //    var result = OnExecuteProcess(context);
            //    Result
            //        .Set
            //            (
            //                context
            //                , result
            //            );
            //}
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

        private void OnBookmarkCallback(NativeActivityContext context, Bookmark bookmark, object state)
        {
            var result = OnResumeBookmarkProcess(context, bookmark);
            Result
                .Set
                    (
                        context
                        , result
                    );

        }
    }
}
#endif