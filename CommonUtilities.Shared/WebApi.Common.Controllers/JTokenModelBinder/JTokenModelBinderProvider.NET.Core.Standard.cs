#if NETCOREAPP2_X
namespace Microshaoft.WebApi.ModelBinders
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web;
    public class JTokenModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext
                                    .HttpContext
                                    .Request;

            JToken jToken = null;
            async void RequestFormBodyProcess()
            {
                if (request.HasFormContentType)
                {
                    var formCollectionModelBinder = new FormCollectionModelBinder(NullLoggerFactory.Instance);
                    await formCollectionModelBinder.BindModelAsync(bindingContext);
                    if (bindingContext.Result.IsModelSet)
                    {
                        jToken = JTokenWebHelper
                                        .ToJToken
                                            (
                                                (IFormCollection)
                                                    bindingContext
                                                            .Result
                                                            .Model
                                            );
                    }
                }
                else
                {
                    //if (request.IsJsonRequest())
                    {
                        using (var streamReader = new StreamReader(request.Body))
                        {
                            var task = streamReader.ReadToEndAsync();
                            await task;
                            var json = task.Result;
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
                var qs = request.QueryString.Value;
                if (qs.IsNullOrEmptyOrWhiteSpace())
                {
                    return;
                }
                qs = HttpUtility
                            .UrlDecode
                                (
                                    qs
                                );
                if (qs.IsNullOrEmptyOrWhiteSpace())
                {
                    return;
                }
                qs = qs.TrimStart('?');
                if (qs.IsNullOrEmptyOrWhiteSpace())
                {
                    return;
                }
                var isJson = false;
                try
                {
                    jToken = JToken.Parse(qs);
                    isJson = jToken is JObject;
                }
                catch
                {

                }
                if (!isJson)
                {
                    jToken = request.Query.ToJToken();
                }
            }
            // 取 jwtToken 优先级顺序：Header → QueryString → Body
            StringValues jwtToken = string.Empty;
            IConfiguration configuration = 
                    (IConfiguration) request
                                        .HttpContext
                                        .RequestServices
                                        .GetService
                                            (
                                                typeof(IConfiguration)
                                            );
            var jwtTokenName = configuration
                                .GetSection("TokenName")
                                .Value;
            var needExtractJwtToken = !jwtTokenName.IsNullOrEmptyOrWhiteSpace();
            void ExtractJwtTokenInJToken()
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
                request
                    .Headers
                    .TryGetValue
                        (
                           jwtTokenName
                           , out jwtToken
                        );
            }
            RequestQueryStringHeaderProcess();
            ExtractJwtTokenInJToken();
            if
                (
                    string.Compare(request.Method, "post", true) == 0
                )
            {
                RequestFormBodyProcess();
                ExtractJwtTokenInJToken();
                //if (jToken == null)
                //{
                //    RequestHeaderProcess();
                //}
            }
            if (!StringValues.IsNullOrEmpty(jwtToken))
            {
                request
                    .HttpContext
                    .Items
                    .Add
                        (
                            jwtTokenName
                            , jwtToken
                        );
            }
            bindingContext
                    .Result =
                        ModelBindingResult
                                .Success
                                    (
                                        jToken
                                    );
        }
    }
    //public class JTokenModelBinderProvider
    //                        : IModelBinderProvider
    //                            , IModelBinder
    //{
    //    private IModelBinder _binder = new JTokenModelBinder();
    //    public async Task BindModelAsync(ModelBindingContext bindingContext)
    //    {
    //        await _binder.BindModelAsync(bindingContext);
    //    }
    //    public IModelBinder GetBinder(ModelBinderProviderContext context)
    //    {
    //        if (context == null)
    //        {
    //            throw new ArgumentNullException(nameof(context));
    //        }
    //        if (context.Metadata.ModelType == typeof(JToken))
    //        {
    //            //_binder = new JTokenModelBinder();
    //            return _binder;
    //        }
    //        return null;
    //    }
    //}
}
#endif