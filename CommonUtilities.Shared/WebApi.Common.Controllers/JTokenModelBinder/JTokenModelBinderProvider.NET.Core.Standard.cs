#if NETCOREAPP2_X
namespace Microshaoft.WebApi.ModelBinders
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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

            async void RequestBodyProcess()
            {
                if (request.HasFormContentType)
                {
                    var formBinder = new FormCollectionModelBinder();
                    await formBinder.BindModelAsync(bindingContext);
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
                    if (request.IsJsonRequest())
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

            void RequestHeaderProcess()
            {
                if (request.IsJsonRequest())
                {
                    var json = request
                                    .QueryString
                                    .Value
                                    .Trim('?');
                    json = HttpUtility
                                    .UrlDecode
                                        (
                                            json
                                        );
                    jToken = JToken.Parse(json);
                }
                else
                {
                    jToken = request.Query.ToJToken();
                }
            }

            if
                (
                    string.Compare(request.Method, "get", true) == 0
                )
            {
                RequestHeaderProcess();
                if (jToken == null)
                {
                    RequestBodyProcess();
                }
            }
            else
                //if 
                //(
                //    string.Compare(request.Method, "get", true) == 0
                //)
            {
                RequestBodyProcess();
                if (jToken == null)
                {
                    RequestHeaderProcess();
                }
                
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
    public class JTokenModelBinderProvider 
                            : IModelBinderProvider
                                , IModelBinder
    {
        private IModelBinder _binder = new JTokenModelBinder();
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            await _binder.BindModelAsync(bindingContext);
        }
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.Metadata.ModelType == typeof(JToken))
            {
                //_binder = new JTokenModelBinder();
                return _binder;
            }
            return null;
        }
    }
}
#endif