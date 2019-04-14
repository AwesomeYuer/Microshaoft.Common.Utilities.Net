using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Microshaoft
{
    public class JTokenWrapper
    {
        //private ConcurrentDictionary<string, JTokenWrapper> _properties = new ConcurrentDictionary<string, JTokenWrapper>();

        public JToken Token = null;
        public JTokenWrapper(JToken jToken)
        {
            Token = jToken;
        }
        //private JTokenWrapper GetOrAddProperty(string propertyName)
        //{
        //    return
        //        _properties
        //            .GetOrAdd
        //                (
        //                    propertyName
        //                    , (x) =>
        //                    {
        //                        var jToken = _jToken[propertyName];
        //                        var r = new JTokenWrapper(jToken);
        //                        return
        //                            r;
        //                    }
        //                );
        //}
        //public JToken this[string propertyName]
        //{
        //    get
        //    {
        //        //var jTokenWrapper = GetOrAddProperty(propertyName);
        //        return jTokenWrapper;
        //    }
        //    set
        //    {
        //        var jTokenWrapper = GetOrAddProperty(propertyName);
        //        jTokenWrapper._jToken[propertyName] = value._jToken;  
        //    }
        //}
        public static JTokenWrapper Parse(string json)
        {
            var jToken = JToken.Parse(json);
            var r = new JTokenWrapper(jToken);
            return r;
        }
        public static T ParseAs<T>(string json)
                     where T : JContainer
        {
            
            var j = JToken.Parse(json);
            var r = (T) j;
            return r;
        }

        public T TokenAs<T>()
            where T : JContainer
        {
            var r = (T) this.Token;
            return r;
        }
        //public T Value<T>()
        //{
        //    return
        //        _jToken.Value<T>();
        //}
    }
}
