#if NETCOREAPP2_X
namespace Microshaoft.WebApi
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Linq;

    public static class JTokenWebHelper
    {
        public static bool IsJsonRequest(this HttpRequest target)
        {
            return target.ContentType == "application/json";
        }
        public static JToken ToJToken(this IQueryCollection target)
        {
            return
                 ToJToken
                    (
                        (IEnumerable<KeyValuePair<string, StringValues>>)
                            target
                    );
        }
        public static JToken ToJToken(this IFormCollection target)
        {
            return
                 ToJToken
                    (
                        (IEnumerable<KeyValuePair<string, StringValues>>)
                            target
                    );
        }
        public static JToken ToJToken
                                (
                                    this
                                        IEnumerable
                                            <KeyValuePair<string, StringValues>>
                                                target
                                )
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
                                        var valueText = x.Value[0];
                                        if (!string.IsNullOrEmpty(valueText))
                                        {
                                            jToken = new JValue(x.Value[0]);
                                        }
                                    }
                                    return
                                            new
                                                JProperty
                                                    (
                                                        x.Key
                                                        , jToken
                                                    );
                                }
                            );
            var result = new JObject(jProperties);
            return result;
        }
    }
}
#endif