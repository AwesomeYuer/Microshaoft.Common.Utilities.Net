#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web;
    using Microshaoft.WebApi;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;

    public static partial class HttpRequestHelper
    {
        public static bool TryParseJTokenParameters
                    (
                        this HttpRequest target
                        , out JToken parameters
                        , out string secretJwtToken
                        , Func<Task<JToken>> onFormProcessFuncAsync = null
                        , string jwtTokenName = "xJwtToken"
                    )
        {
            JToken jToken = null;
            void RequestFormBodyProcess()
            {
                if (target.HasFormContentType)
                {
                    if (onFormProcessFuncAsync != null)
                    {
                        jToken = onFormProcessFuncAsync().Result;
                    }
                }
                else
                {
                    //if (request.IsJsonRequest())
                    {
                        using (var streamReader = new StreamReader(target.Body))
                        {
                            var json = streamReader.ReadToEnd();
                            if (!json.IsNullOrEmptyOrWhiteSpace())
                            {
                                jToken = JToken.Parse(json);
                            }
                        }
                    }
                }
            }
            void RequestQueryStringHeaderProcess()
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
            void ExtractJwtToken()
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
            RequestQueryStringHeaderProcess();
            ExtractJwtToken();
            if
                (
                    string.Compare(target.Method, "get", true) != 0
                    &&
                    string.Compare(target.Method, "head", true) != 0
                )
            {
                RequestFormBodyProcess();
                ExtractJwtToken();
                //if (jToken == null)
                //{
                //    RequestHeaderProcess();
                //}
            }
            parameters = jToken;
            secretJwtToken = jwtToken;
            bool r = true;
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