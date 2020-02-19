namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public static partial class HttpClientHelper
    {
        public static IEnumerable<HttpContent> GetContentsAsEnumerable(this HttpResponseMessage target)
        {
            var multipartMemoryStreamProvider = target.Content.ReadAsMultipartAsync().Result;
            var contents = multipartMemoryStreamProvider.Contents;
            foreach (var content in contents)
            {
                yield
                    return
                        content;
            }
        }
        private static Encoding GetEncodingOrDefault
                                        (
                                            string encodingName = null
                                        )
        {
            Encoding encoding;
            if (encodingName == null)
            {
                encoding = Encoding.UTF8;
            }
            else
            {
                encoding = Encoding.GetEncoding(encodingName);
            }
            return
                encoding;

        }


        public static IEnumerable<string> GetContentBodyStringsAsEnumerable
                                                (
                                                    this HttpResponseMessage target
                                                    , string encodingName = null
                                                    , bool detectEncodingFromByteOrderMarks = false
                                                )
        {
            var multipartMemoryStreamProvider = target.Content.ReadAsMultipartAsync().Result;
            var contents = multipartMemoryStreamProvider.Contents;
            Encoding encoding = GetEncodingOrDefault(encodingName);
            foreach (var content in contents)
            {
                using (var stream = content.ReadAsStreamAsync().Result)
                {
                    using (var streamReader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks))
                    {
                        string s;
                        do
                        {
                            s = streamReader.ReadLine();
                        }
                        while
                            (
                                s.Length > 0
                            );
                        s = streamReader.ReadToEnd();
                        streamReader.Close();
                        stream.Close();
                        yield
                            return
                                s;
                    }
                }
            }
        }

        public static async IAsyncEnumerable<string> GetContentBodyStringsAsAsyncEnumerable
                                                            (
                                                                this HttpResponseMessage target
                                                                , string encodingName = null
                                                                , bool detectEncodingFromByteOrderMarks = false
                                                            )
        {
            var multipartMemoryStreamProvider = await target.Content.ReadAsMultipartAsync();
            var contents = multipartMemoryStreamProvider.Contents;
            Encoding encoding = GetEncodingOrDefault(encodingName);
            foreach (var content in contents)
            {
                using (var stream = await content.ReadAsStreamAsync())
                {
                    using (var streamReader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks))
                    {
                        string s;
                        do
                        {
                            s = await streamReader.ReadLineAsync();
                        }
                        while
                            (
                                s.Length > 0
                            );
                        s = await
                                streamReader
                                        .ReadToEndAsync();
                        streamReader.Close();
                        stream.Close();
                        yield
                            return
                                s;
                    }
                }
            }
        }
        public static Task<HttpResponseMessage> SendBatchAsync
                                                    (
                                                        this HttpClient target
                                                        , string relativeUrl
                                                        , params HttpRequestMessage[] httpRequestMessages
                                                    )
        {
            var httpRequestMessage = CreateBatchRequest(relativeUrl, httpRequestMessages);
            if (httpRequestMessage != null)
            {
                return
                    target.SendAsync(httpRequestMessage);
            }
            else
            {
                return null;
            }
        }

        public static HttpRequestMessage CreateBatchRequest
                                                    (
                                                        string relativeUrl
                                                        , params HttpRequestMessage[] httpRequestMessages
                                                    )
        {
            MultipartContent multipartContent = null;
            foreach (var httpRequestMessage in httpRequestMessages)
            {
                if (multipartContent == null)
                {
                    multipartContent = new MultipartContent("mixed", "batch_" + Guid.NewGuid().ToString());
                }
                var httpMessageContent = new HttpMessageContent(httpRequestMessage);
                multipartContent.Add(httpMessageContent);
            }
            HttpRequestMessage batchHttpRequestMessage = null;
            if (multipartContent != null)
            {
                batchHttpRequestMessage = new HttpRequestMessage(HttpMethod.Post, relativeUrl)
                {
                    Content = multipartContent
                };
            }
            return batchHttpRequestMessage;
        }
    }
}
namespace Microshaoft.Tests
{
    using Microshaoft;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;


    class Program
    {
        const string serviceBaseAddress = "http://localhost:9000/";
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(serviceBaseAddress);
            var response = await client
                                    .SendBatchAsync
                                        (
                                            "api/asyncbatch"
                                            , new HttpRequestMessage(HttpMethod.Get, client.BaseAddress + "get1?id=1111")
                                            , new HttpRequestMessage(HttpMethod.Get, client.BaseAddress + "get2?id=2222")
                                        );
            var headers = JsonConvert.SerializeObject(response.Headers);
            Console.WriteLine($"{nameof(headers)}:{headers}");
            var ss = response.GetContentBodyStringsAsEnumerable();
            foreach (var s in ss)
            {
                Console.WriteLine(s);
            }

            var sss = response.GetContentBodyStringsAsAsyncEnumerable();
            await foreach (var s in sss)
            {
                Console.WriteLine(s);
            }
            Console.WriteLine(response.StatusCode);

            Console.ReadLine();
        }
    }
}