#if NETCOREAPP2_X

namespace Microshaoft.WebApi
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class JTokenWebExtensions
    {
        public static JToken ToJToken(this IFormCollection target)
        {
            return
                JTokenWebHelper.ToJToken(target);
        }
        public static JToken ToJToken(this IQueryCollection target)
        {
            return
                JTokenWebHelper.ToJToken(target);
        }
    }

    public static class JTokenWebHelper
    {
        public static JToken ToJToken(IQueryCollection target)
        {
            return
                 ToJToken((IEnumerable<KeyValuePair<string, StringValues>>)target);



        }
        public static JToken ToJToken(IFormCollection target)
        {
            return
                 ToJToken((IEnumerable<KeyValuePair<string, StringValues>>)target);


        }


        private static JToken ToJToken(IEnumerable<KeyValuePair<string, StringValues>> target)
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

        }

    }
}
#endif