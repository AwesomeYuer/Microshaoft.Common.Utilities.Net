#if NETCOREAPP
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Net;
    using System.Text.Json;
    using SystemJsonSerializer = System.Text.Json.JsonSerializer;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http.Features;

    public /*sealed */ class ExceptionOnDemandHandlerMiddleware
    {
        private const string defaultErrorMessage = nameof(HttpStatusCode.InternalServerError);
        private const string defaultErrorResponseContentType = "application/json";
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions defaultJsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        public ExceptionOnDemandHandlerMiddleware
                            (
                                IWebHostEnvironment env
                                , IConfiguration configuration
                            )
        {
            _env = env;
            _configuration = configuration;
        }

        public Func
                    <
                        HttpContext
                        , IConfiguration
                        , Exception
                        , DateTime?
                        , string
                        , Guid?
                        ,
                            (
                                bool                // error Details
                                , HttpStatusCode
                                , int               // error Result Code
                                , string            // error Message
                            )
                    >
                        OnCaughtExceptionHandleProcess;

        public async Task Invoke(HttpContext context)
        {
            var response = context
                                .Response;
            var exception = context
                                .Features
                                .Get<IExceptionHandlerFeature>()?
                                .Error;
            if (exception != null)
            {
                var errorDetails = false;
                var errorStatusCode = HttpStatusCode.InternalServerError;
                var errorResultCode = -1 * (int) errorStatusCode;
                var errorMessage = defaultErrorMessage;
                if (OnCaughtExceptionHandleProcess != null)
                {
                    DateTime? errorTime = DateTime.Now;
                    var httpRequestFeature = context
                                                    .Features
                                                    .Get<IHttpRequestFeature>();
                    string requestResponseTimingItemKey = nameof(requestResponseTimingItemKey);
                    DateTime beginTime = default;
                    long beginTimeStamp = default;
                    Guid? traceID = default;
                    if
                        (
                            context
                                .Items
                                .TryGetValue
                                    (
                                        requestResponseTimingItemKey
                                        , out var v
                                    )
                        )
                    {
                        (
                            beginTime
                            , beginTimeStamp
                            , traceID
                        )
                        =
                            (ValueTuple<DateTime, long, Guid>)v;

                    }
                    var requestUrl = httpRequestFeature.RawTarget;
                    (
                        errorDetails
                        , errorStatusCode
                        , errorResultCode
                        , errorMessage
                    )
                    =
                    OnCaughtExceptionHandleProcess
                                (
                                    context
                                    , _configuration
                                    , exception
                                    , errorTime
                                    , requestUrl
                                    , traceID
                                );
                }
                response.StatusCode = (int) errorStatusCode;
                response.ContentType = defaultErrorResponseContentType;
                if (errorDetails && errorMessage.IsNullOrEmptyOrWhiteSpace())
                {
                    errorMessage = exception.ToString();
                }
                var jsonResult = new
                {
                    statusCode = errorStatusCode
                    , resultCode = errorResultCode
                    , message = errorMessage
                };
                await
                    SystemJsonSerializer
                                .SerializeAsync
                                        (
                                            response.Body
                                            , jsonResult
                                            , defaultJsonSerializerOptions
                                        );
            }
        }
    }
}
#endif