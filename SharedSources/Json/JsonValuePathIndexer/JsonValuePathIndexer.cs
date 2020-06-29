namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    internal class Program
    {
        static void Main(string[] args)
        {
            var json = $@"{{F1:1,F3:0.1, F2:{{aaa:[""{DateTime.Now}""]}}}}";


            var jta = JsonValuePathIndexer.Parse(json);
            Console.WriteLine(jta.PathIndexer<DateTime>()["F2.aaa[0]"]);
            Console.WriteLine(jta.PathIndexer<DateTime>()["F2.aaa[0]"]);
            Console.WriteLine(jta.PathIndexer<Double>()["F3"]);
            jta.PathIndexer<Double>()["F3"] = 999;
            Console.WriteLine(jta.PathIndexer<Double>()["F3"]);

            Console.ReadLine();
        }
    }

    public class JsonValuePathIndexer
    {
        private readonly JToken _token;
        public JToken Token => _token;

        private ConcurrentDictionary<Type, object> _cache
                    = new ConcurrentDictionary<Type, object>();
        public static JsonValuePathIndexer Parse(string json)
        {
            return
                new JsonValuePathIndexer(json);
        }

        public JsonValuePathIndexer(string json)
        {
            _token = JToken.Parse(json);
        }
        public JsonValuePathIndexer(JToken jToken)
        {
            _token = jToken;
        }
        public JsonValuePathIndexer<T> PathIndexer<T>()
        {
            return
                (JsonValuePathIndexer<T>)
                        _cache
                            .GetOrAdd
                                (
                                    typeof(T)
                                    , (xType) =>
                                    {
                                        return
                                            new JsonValuePathIndexer<T>(Token);
                                    }
                                );
        }
    }
    public class JsonValuePathIndexer<T> //: JToken
    {
        private readonly JToken _token;

        public JToken Token => _token;

        public JsonValuePathIndexer(JToken jToken)
        {
            _token = jToken;
        }
        private T GetValue(string path)
        {
            return
                Token
                    .SelectToken(path)
                    .Value<T>();
        }
        private bool SetValue(string path, T @value)
        {
            bool r = false;
            var jToken = Token
                            .SelectToken(path)
                            .Parent;
            if (jToken is JProperty jProperty)
            {
                jProperty.Value = new JValue(@value);
                r = true;
            }
            return r;
        }
        public T this[string path]
        {
            get
            {
                return
                   GetValue(path);
            }
            set
            {
                SetValue(path, value);
            }
        }
    }
}
