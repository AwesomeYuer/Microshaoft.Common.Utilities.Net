namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;

    public class JTokenWrapper
    {
        //private ConcurrentDictionary<string, JTokenWrapper> _properties = new ConcurrentDictionary<string, JTokenWrapper>();

        public JToken Token = null;
        public JTokenWrapper(JToken jToken)
        {
            Token = jToken;
        }
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
            var r = (T) Token;
            return r;
        }


        public static explicit operator JToken(JTokenWrapper target)
        {
            return target.Token;
        }


        public static explicit operator JObject(JTokenWrapper target)
        {
            if (target.Token is JObject j)
            {
                return j;
            }
            throw new Exception($"target is not a JObject");
        }

        public static explicit operator JArray(JTokenWrapper target)
        {
            if (target.Token is JArray jA)
            {
                return jA;
            }
            throw new Exception($"target is not a JArray");
        }
    }
}
