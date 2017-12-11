#if NETFRAMEWORK4_X

namespace Test
{
    using System;
    using Microshaoft;
    class Program1
    {
        const int BufferSize = 1024;

        static readonly string _baseAddress = "http://localhost:50231/";
        static readonly string _filename = "Sample.xml";
        static void Main(string[] args)
        {
            HttpClientHelper
                .PostUploadFile
                    (
                        @"E:\download\PerfView.zip"
                        , @"http://localhost:10281/api/restful/Files/upload"
                        , (x, y, z) =>
                        {
                            Console.WriteLine("{1}{0}{2}", ":", y, z.ProgressPercentage);
                        }
                        //, null
                        , false
                        , (x, y, z) =>
                        {
                            return false;
                        }
                        , null
                    );
            Console.ReadLine();
        }
    }
}


namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Handlers;

    using System.Threading.Tasks;
    public static class HttpClientHelper
    {
        //[Conditional("NETFRAMEWORK4_X")]
        public static void RegisterHttpClientProgressMessageHandler
                                (
                                    Action<HttpClient> onHttpClientSendReceiveProcessAction
                                    , Action
                                        <
                                            HttpClient
                                            , bool
                                            , HttpProgressEventArgs
                                        >
                                            onProgressProcessAction = null
                                    , bool reThrowException = false
                                    , Func<Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                                    , Action<bool, Exception, Exception, string> onFinallyProcessAction = null
                                )
        {
            HttpClientHandler httpClientHandler = null;
            HttpClient httpClient = null;
            ProgressMessageHandler progressMessageHandler = null;
            EventHandler<HttpProgressEventArgs> httpSendProgress = null;
            EventHandler<HttpProgressEventArgs> httpReceiveProgress = null;
            var needProgress = false;
            try
            {
                if (onProgressProcessAction != null)
                {
                    needProgress = true;
                    httpClientHandler = new HttpClientHandler();
                    progressMessageHandler = new ProgressMessageHandler(httpClientHandler);
                    httpSendProgress = new EventHandler<HttpProgressEventArgs>
                                            (
                                                (sender, httpProgressEventArgs) =>
                                                {
                                                    onProgressProcessAction
                                                        (
                                                            httpClient
                                                            , true
                                                            , httpProgressEventArgs
                                                        );
                                                }
                                            );
                    httpReceiveProgress = new EventHandler<HttpProgressEventArgs>
                                            (
                                                (sender, httpProgressEventArgs) =>
                                                {
                                                    onProgressProcessAction
                                                        (
                                                            httpClient
                                                            , false
                                                            , httpProgressEventArgs
                                                        );
                                                }
                                            );
                    progressMessageHandler.HttpSendProgress += httpSendProgress;
                    progressMessageHandler.HttpReceiveProgress += httpReceiveProgress;
                }
                using
                    (
                        httpClient = (needProgress ? new HttpClient(progressMessageHandler) : new HttpClient())
                    )
                {

                    TryCatchFinallyProcessHelper
                        .TryProcessCatchFinally
                            (
                                true
                                , () =>
                                {
                                    onHttpClientSendReceiveProcessAction(httpClient);
                                }
                                , false
                                , (exception, newException, message) =>
                                {

                                    if (onCaughtExceptionProcessFunc != null)
                                    {
                                        reThrowException = onCaughtExceptionProcessFunc
                                                                (
                                                                    exception
                                                                    , newException
                                                                    , message
                                                                );
                                    }
                                    if (reThrowException)
                                    {
                                        throw
                                            newException;
                                    }
                                    return false;
                                }
                                , (caughtException, Exception, newException, message) =>
                                {
                                    onFinallyProcessAction?
                                        .Invoke
                                            (
                                                caughtException
                                                , Exception
                                                , newException
                                                , message
                                            );
                                }
                            );
                }
            }
            finally
            {
                if (progressMessageHandler != null)
                {
                    progressMessageHandler.HttpSendProgress -= httpSendProgress;
                    progressMessageHandler.HttpReceiveProgress -= httpReceiveProgress;
                    progressMessageHandler.Dispose();
                }
                if (httpClientHandler != null)
                {
                    httpClientHandler.Dispose();
                }
            }
        }
        public static void PostUploadFile
                            (
                                string file
                                , string url
                                , Action
                                    <
                                        HttpClient
                                        , bool
                                        , HttpProgressEventArgs
                                    >
                                    onProgressProcessAction = null
                                , bool reThrowException = false
                                , Func<Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                                , Action<bool, Exception, Exception, string> onFinallyProcessAction = null
                                , int httpClientTimeOutInSeconds = 200
                                , int bufferSize = 64 * 1024
                            )
        {
            RegisterHttpClientProgressMessageHandler
                    (
                        (httpClient) =>
                        {
                            HttpResponseMessage response = null;
                            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
                            {
                                var streamContent = new StreamContent(fileStream, bufferSize);
                                var multipartFormDataContent = new MultipartFormDataContent();
                                multipartFormDataContent.Add(new StringContent("Me"), "submitter");
                                multipartFormDataContent.Add(streamContent, "filename", file);
                                var uri = new Uri(url);
                                response = httpClient
                                                .PostAsync
                                                    (
                                                        uri
                                                        , multipartFormDataContent
                                                    ).Result;
                            }
                            response.EnsureSuccessStatusCode();
                        }
                        , onProgressProcessAction
                        , reThrowException
                        , onCaughtExceptionProcessFunc
                        , onFinallyProcessAction
                    );
        }
        public static void TryParallelHttpClientsSendData
                     (
                        IEnumerable<string> urls
                        , bool isAsyncWithoutWaitResult = false
                        , string httpMethod = "Get"     //HttpMethod.Get.Method
                        , string contentData = null     //string.Empty
                        , Action
                                <
                                    HttpClient
                                    , bool
                                    , HttpProgressEventArgs
                                >
                                    onProgressProcessAction = null
                        , bool reThrowException = false
                        , Func<Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                        , Action<bool, Exception, Exception, string> onFinallyProcessAction = null
                        , int httpClientTimeOutInSeconds = 5
                        , int maxDegreeOfParallelism = 4
                     )
        {
            Parallel
                .ForEach
                    (
                        urls
                        , new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = maxDegreeOfParallelism//Environment
                                                                           //    .ProcessorCount
                        }
                        , (x) =>
                        {
                            if (isAsyncWithoutWaitResult)
                            {
                                //如果允许异步执行不等待

                                SendDataWithoutWaitResultAsync
                                    (
                                        x
                                        , httpMethod
                                        , contentData
                                        , onProgressProcessAction
                                        , reThrowException
                                        , onCaughtExceptionProcessFunc
                                        , onFinallyProcessAction
                                        , httpClientTimeOutInSeconds
                                    );

                            }
                            else
                            {
                                //如果同步执行
                                SendData
                                    (
                                        x
                                        , httpMethod
                                        , contentData
                                        , onProgressProcessAction
                                        , reThrowException
                                        , onCaughtExceptionProcessFunc
                                        , onFinallyProcessAction
                                        , httpClientTimeOutInSeconds
                                    );
                            }
                        }
                    );
        }

        /// <summary>
        /// 异步执行不等待
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <param name="contentData"></param>
        /// <param name="onProgressProcessAction"></param>
        /// <param name="httpClientTimeOutInSeconds"></param>
        public static async Task SendDataWithoutWaitResultAsync
                                    (
                                        string url
                                        , string httpMethod = "Get"
                                        , string contentData = null
                                        , Action
                                                <
                                                    HttpClient
                                                    , bool
                                                    , HttpProgressEventArgs
                                                >
                                            onProgressProcessAction = null
                                        , bool reThrowException = false
                                        , Func<Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                                        , Action<bool, Exception, Exception, string> onFinallyProcessAction = null
                                        , int httpClientTimeOutInSeconds = 5
                                    )
        {
            RegisterHttpClientProgressMessageHandler
                 (
                    async (httpClient) =>
                    {
                        HttpResponseMessage response = null;
                        //Task<HttpResponseMessage> task = null;
                        if (httpMethod.Trim().ToLower() == "get")
                        {
                            response =
                            await httpClient
                                        .GetAsync
                                            (
                                                url
                                            );
                        }
                        else if (httpMethod.Trim().ToLower() == "post")
                        {
                            StringContent stringContent = null;
                            if (!string.IsNullOrEmpty(contentData))
                            {
                                stringContent = new StringContent(contentData);
                            }
                            response = await httpClient
                                        .PostAsync
                                                (
                                                    url
                                                    , stringContent
                                                );
                            //.ContinueWith
                            //            (
                            //                (x) =>
                            //                {
                            //                    var taskExceptionMessage = "ContinueWith OnlyOnFaulted Caught Exception";
                            //                    var taskException = x.Exception;
                            //                    taskException = taskException.Flatten();

                            //                    var newException = new Exception(taskExceptionMessage, taskException);

                            //                    taskExceptionMessage = string.Format("{1}{0}{2}", ":\r\n", taskExceptionMessage, taskException.ToString());
                            //                    if (onCaughtExceptionProcessFunc != null)
                            //                    {
                            //                        onCaughtExceptionProcessFunc(taskException, newException, taskExceptionMessage);
                            //                    }
                            //                }
                            //                , TaskContinuationOptions.OnlyOnFaulted
                            //            );
                        }

                        //Console.WriteLine("await ed");
                        response.EnsureSuccessStatusCode();
                    }
                    , onProgressProcessAction
                    , reThrowException
                    , onCaughtExceptionProcessFunc
                    , onFinallyProcessAction
                 );
        }

        /// <summary>
        /// 同步执行等待
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpMethod"></param>
        /// <param name="contentData"></param>
        /// <param name="onProgressProcessAction"></param>
        /// <param name="httpClientTimeOutInSeconds"></param>
        public static HttpResponseMessage SendData
                            (
                                string url
                                , string httpMethod = "Get"
                                , string contentData = null
                                , Action
                                        <
                                            HttpClient
                                            , bool
                                            , HttpProgressEventArgs
                                        >
                                    onProgressProcessAction = null
                                , bool reThrowException = false
                                , Func<Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                                , Action<bool, Exception, Exception, string> onFinallyProcessAction = null
                                , int httpClientTimeOutInSeconds = 5
                            )
        {
            HttpResponseMessage response = null;
            RegisterHttpClientProgressMessageHandler
                     (
                        (httpClient) =>
                        {

                            if (httpMethod.Trim().ToLower() == "get")
                            {
                                response = httpClient
                                                    .GetAsync
                                                        (
                                                            url
                                                        )
                                                    .Result;
                            }
                            else if (httpMethod.Trim().ToLower() == "post")
                            {
                                StringContent stringContent = null;
                                if (!string.IsNullOrEmpty(contentData))
                                {
                                    stringContent = new StringContent(contentData);
                                }
                                response = httpClient
                                                    .PostAsync
                                                        (
                                                            url
                                                            , stringContent
                                                        )
                                                    .Result;

                                
                            }
                        }
                        , onProgressProcessAction
                        , reThrowException
                        , onCaughtExceptionProcessFunc
                        , onFinallyProcessAction
                     );
            return response;
        }

    }
}

#endif
