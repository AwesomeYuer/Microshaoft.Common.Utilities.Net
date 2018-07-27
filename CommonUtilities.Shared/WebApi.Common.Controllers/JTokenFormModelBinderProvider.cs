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


    public static class JTokenWebExtensions
    {
        public static JToken ToJToken(this IFormCollection target)
        {
            return
                JTokenWebHelper.ToJToken(target);
        }
    }

    public static class JTokenWebHelper
    {

        public static JToken ToJToken( IFormCollection target)
        {
            IEnumerable<JProperty>
                jProperties
                        = target
                            .Select
                                (
                                    (x) =>
                                    {
                                        JToken jToken = null;
                                        if (x.Value.Count() > 1)
                                        {
                                            jToken = new JArray(x.Value);
                                        }
                                        else
                                        {
                                            jToken = new JValue(x.Value[0]);
                                        }
                                        return
                                            new JProperty
                                                (
                                                    x.Key
                                                    , jToken
                                                );
                                    }
                                );

            var result = new JObject(jProperties);
            return result;
            //foreach (var element in form)
            //{
            //    Add(target, element.Key, element.Value);
            //}
            //if (target.Count == 1 && target[""] != null)
            //{
            //    return target[""];
            //}
            //return target;
            //void Add(JObject jo, string key, StringValues value)
            //{
            //    var chars = new[] { '.', '[' };
            //    var x = key.IndexOfAny(chars);
            //    if (x == -1)
            //    {
            //        jo[key] = value.LastOrDefault();
            //    }
            //    else
            //    {
            //        var name = key.Substring(0, x);
            //        if (key[x] == '.')
            //        {
            //            var subJo = jo[name] as JObject ?? (JObject)(jo[name] = new JObject());
            //            Add(subJo, key.Substring(x + 1), value);
            //        }
            //        else
            //        {
            //            var subJa = jo[name] as JArray ?? (JArray)(jo[name] = new JArray());
            //            var closeBracketsIndex = key.IndexOf(']', x + 1);
            //            var itemIndex = 
            //                        int
            //                            .Parse
            //                                (
            //                                    key.Substring(x + 1, closeBracketsIndex - x - 1)
            //                                );
            //            while (subJa.Count < itemIndex + 1)
            //            {
            //                subJa.Add(null);
            //            }
            //            if (closeBracketsIndex == key.Length - 1)
            //            {
            //                subJa[itemIndex] = value.LastOrDefault();
            //                return;
            //            }
            //            if (key[closeBracketsIndex + 1] != '.')
            //            {
            //                throw new Exception();
            //            }
            //            var remainder = key.Substring(closeBracketsIndex + 2);
            //            var subJo =
            //                    (
            //                        (subJa[itemIndex] as JObject)
            //                        ??
            //                        (JObject)(subJa[itemIndex] = new JObject())
            //                    );
            //            Add(subJo, remainder, value);
            //        }
            //    }
            //}
        }

    }


    public class JTokenModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext
                                    .HttpContext
                                    .Request;
            JToken jToken = null;
            if (string.Compare(request.Method, "post", true) == 0)
            {
                if (bindingContext.HttpContext.Request.HasFormContentType)
                {
                    var formBinder = new FormCollectionModelBinder();
                    await formBinder.BindModelAsync(bindingContext);
                    if (bindingContext.Result.IsModelSet)
                    {
                        jToken = JTokenWebHelper.ToJToken
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
                    if (request.ContentType == "application/json")
                    {
                        using (var streamReader = new StreamReader(request.Body))
                        {
                            var task = streamReader.ReadToEndAsync();
                            await task;
                            var json = task.Result;
                            jToken = JToken.Parse(json);
                        }
                    }
                }
            }
            else if (string.Compare(bindingContext.HttpContext.Request.Method, "get", true) == 0)
            {
                if (bindingContext.HttpContext.Request.ContentType == "application/json")
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
                    var json = request.Query["p"][0];
                    if (!json.IsNullOrEmptyOrWhiteSpace())
                    {
                        json = HttpUtility
                                    .UrlDecode
                                            (
                                                json
                                            );
                        jToken = JToken.Parse(json);
                    }
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
    public class JTokenFormModelBinderProvider 
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