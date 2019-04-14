namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
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
    }
}
