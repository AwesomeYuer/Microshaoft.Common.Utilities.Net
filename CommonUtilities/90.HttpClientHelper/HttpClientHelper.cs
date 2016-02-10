

namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Net.Http;
    public static class HttpClientHelper
    {
        public static void ParallelHttpClientsSendPostWithoutData
                     (
                         string relativeUrl
                         , IEnumerable<string> hostsAddresses
                         , int httpClientTimeOutInSeconds = 5
                     )
        {
            ParallelHttpClientsSend
                    (
                        relativeUrl
                        , hostsAddresses
                        , "post"
                        , httpClientTimeOutInSeconds
                    );
        
        }

        public static void ParallelHttpClientsSendGet
             (
                 string relativeUrl
                 , IEnumerable<string> hostsAddresses
                 , int httpClientTimeOutInSeconds = 5
             )
        {
            ParallelHttpClientsSend
                    (
                        relativeUrl
                        , hostsAddresses
                        , "get"
                        , httpClientTimeOutInSeconds
                    );

        }


        private static void ParallelHttpClientsSend
                     (
                         string relativeUrl
                         , IEnumerable<string> hostsAddresses
                         , string httpMethod
                         , int httpClientTimeOutInSeconds = 5
                     )
        {
            Parallel
                .ForEach
                    (
                        hostsAddresses
                        , new ParallelOptions()
                        {
                            MaxDegreeOfParallelism = Environment
                                                            .ProcessorCount
                        }
                        , (x, y, z) =>
                        {
                            var url = x;
                            if (!url.StartsWith("http://"))
                            {
                                url = string.Format("http://{0}", url);
                            }
                            url += relativeUrl;
                            using
                                (
                                    HttpClient httpClient
                                                    =
                                                        new HttpClient()
                                                        {
                                                            Timeout = TimeSpan
                                                                        .FromSeconds
                                                                            (
                                                                                httpClientTimeOutInSeconds
                                                                            )
                                                        }
                                )
                            {
                                HttpResponseMessage response = null;
                                if (httpMethod.Trim().ToLower() == "get")
                                {
                                    response = httpClient
                                                        .GetAsync
                                                            (
                                                                url
                                                            ).Result;
                                }
                                else if (httpMethod.Trim().ToLower() == "post")
                                {
                                    response = httpClient
                                                            .PostAsync
                                                                (

                                                                    url
                                                                    , null
                                                                ).Result;
                                }

                                
                            }
                        }
                    );
        }
    }
}
