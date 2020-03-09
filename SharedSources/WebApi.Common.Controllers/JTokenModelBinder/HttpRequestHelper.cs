#if NETCOREAPP
namespace Microshaoft.Web
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web;
    //using Microshaoft.Linq.Dynamic;
    using Microshaoft.WebApi;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;

    public static partial class HttpRequestHelper
    {
        public static JsonResult NewJsonResult
                                (
                                    int statusCode
                                    , int resultCode
                                    , string message
                                )
        {
            return
                new JsonResult
                        (
                            new
                            {
                                statusCode
                                , resultCode
                                , message
                            }
                        )
                    {
                        StatusCode = statusCode
                        , ContentType = "application/json"
                    };
        }

        public static void SetJsonResult
                                (
                                    this ActionExecutingContext target
                                    , int statusCode
                                    , int resultCode
                                    , string message
                                )
        {
            target
                .Result = NewJsonResult
                            (
                                statusCode
                                , resultCode
                                , message
                            );
        }
        public static void SetJsonResult
                        (
                            this ActionExecutedContext target
                            , int statusCode
                            , int resultCode
                            , string message
                        )
        {
            target
                .Result = NewJsonResult
                            (
                                statusCode
                                , resultCode
                                , message
                            );
        }
        public static void SetNotFoundJsonResult
                                (
                                    this ActionExecutedContext target
                                    , int? resultCode = null
                                    , string message = null
                                )
        {
            
            var statusCode = 404;
            if (resultCode == null || !resultCode.HasValue)
            {
                resultCode = -1 * statusCode;
            }
            if (message.IsNullOrEmptyOrWhiteSpace())
            {
                var request = target.HttpContext.Request;
                message = $"{request.Path.Value} not found!";
            }
            target
                .Result = NewJsonResult
                            (
                                statusCode
                                , resultCode.Value
                                , message
                            );
        }
        public static void SetNotFoundJsonResult
                        (
                            this ActionExecutingContext target
                            , int? resultCode = null
                            , string message = null
                        )
        {
            var statusCode = 404;
            if (resultCode == null || !resultCode.HasValue)
            {
                resultCode = -1 * statusCode;
            }
            if (message.IsNullOrEmptyOrWhiteSpace())
            {
                var request = target.HttpContext.Request;
                message = $"{request.Path.Value} not found!";
            }
            target
                .Result = NewJsonResult
                            (
                                statusCode
                                , resultCode.Value
                                , message
                            );
        }

        public static string GetActionRoutePath
                    (
                        this HttpRequest target
                        , string key = " "
                    )
        {
            return
                target
                    .RouteValues[key]
                    .ToString();
        }

        public static string GetActionRoutePathOrDefault
            (
                this HttpRequest target
                , string defaultValue = default
                , string key = " "
            )
        {
            var r = string.Empty;
            if
                (
                    TryGetActionRoutePath
                        (
                            target
                            , out var @value
                            , key
                        )
                )
            {
                r = @value;
            }
            else
            {
                r = defaultValue;
            }
            return
                r;
        }
        public static bool TryGetActionRoutePath
            (
                this HttpRequest target
                , out string @value
                , string key = " "
            )
        {
            @value = string.Empty;
            var r = false;
            if
                (
                    target
                        .RouteValues
                        .TryGetValue
                            (
                                key
                                , out var @object
                            )
                    &&
                    @object != null
                )
            {
                if (@object is string s)
                {
                    if (!s.IsNullOrEmptyOrWhiteSpace())
                    {
                        @value = s;
                        r = true;
                    }
                }
            }
            return
                r;
        }

        public static bool TryParseJTokenParameters
                    (
                        this HttpRequest target
                        , out JToken parameters
                        , out string secretJwtToken
                        , Func<Task<JToken>> onFormProcessFuncAsync = null
                        , string jwtTokenName = "xJwtToken"
                    )
        {
            bool r;
            JToken jToken = null;
            void requestFormBodyProcess()
            {
                if 
                    (
                        !target.IsJsonRequest()
                        &&
                        target.HasFormContentType
                    )
                {
                    if (onFormProcessFuncAsync != null)
                    {
                        jToken = onFormProcessFuncAsync().Result;
                    }
                }
                else
                {
                    using var streamReader = new StreamReader(target.Body);
                    var json = streamReader.ReadToEnd();
                    if (!json.IsNullOrEmptyOrWhiteSpace())
                    {
                        jToken = JToken.Parse(json);
                    }
                }
            }
            void requestQueryStringHeaderProcess()
            {
                var queryString = target.QueryString.Value;
                if (queryString.IsNullOrEmptyOrWhiteSpace())
                {
                    return;
                }
                queryString = HttpUtility
                                    .UrlDecode
                                        (
                                            queryString
                                        );
                if (queryString.IsNullOrEmptyOrWhiteSpace())
                {
                    return;
                }
                queryString = queryString.TrimStart('?');
                if (queryString.IsNullOrEmptyOrWhiteSpace())
                {
                    return;
                }
                var isJson = false;
                try
                {
                    if (queryString.IsJson(out jToken, true))
                    {
                        isJson = jToken is JObject;
                    }
                }
                catch
                {

                }
                if (!isJson)
                {
                    jToken = target.Query.ToJToken();

                    //Console.WriteLine("target.Query.ToJToken()");
                }
            }
            // 取 jwtToken 优先级顺序：Header → QueryString → Body
            StringValues jwtToken = string.Empty;
            var needExtractJwtToken = !jwtTokenName.IsNullOrEmptyOrWhiteSpace();
            void extractJwtToken()
            {
                if (needExtractJwtToken)
                {
                    if (jToken != null)
                    {
                        if (StringValues.IsNullOrEmpty(jwtToken))
                        {
                            var j = jToken[jwtTokenName];
                            if (j != null)
                            {
                                jwtToken = j.Value<string>();
                            }
                        }
                    }
                }
            }
            if (needExtractJwtToken)
            {
                target
                    .Headers
                    .TryGetValue
                            (
                               jwtTokenName
                               , out jwtToken
                            );
            }
            requestQueryStringHeaderProcess();
            extractJwtToken();
            if
                (
                    string.Compare(target.Method, "get", true) != 0
                    &&
                    string.Compare(target.Method, "head", true) != 0
                )
            {
                requestFormBodyProcess();
                extractJwtToken();
                //if (jToken == null)
                //{
                //    RequestHeaderProcess();
                //}
            }
            parameters = jToken;
            secretJwtToken = jwtToken;
            r = true;
            return r;
        }

        public static async 
            Task<JToken> GetFormJTokenAsync
                                (
                                    this ModelBindingContext target
                                )
        {
            JToken r = null;
            var formCollectionModelBinder =
                                new FormCollectionModelBinder
                                        (
                                            NullLoggerFactory
                                                        .Instance
                                        );
            await
                formCollectionModelBinder
                            .BindModelAsync(target);
            if 
                (
                    target
                        .Result
                        .IsModelSet
                )
            {
                r = JTokenWebHelper
                                .ToJToken
                                    (
                                        (IFormCollection)
                                            target
                                                .Result
                                                .Model
                                    );
            }
            return r;
        }
    }
}
#endif