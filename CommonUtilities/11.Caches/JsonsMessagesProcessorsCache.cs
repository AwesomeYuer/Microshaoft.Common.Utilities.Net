//#if NET45
namespace Microsoft.Boc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;
    using Microsoft.Boc.Share;
    using Newtonsoft.Json.Linq;
    /// <summary>
    /// 类型序列化器缓存
    /// </summary>
    public static class JsonsMessagesProcessorsCacheManager
    {
        private static JsonsMessagesProcessors _jsonsMessagesProcessorsCache = new JsonsMessagesProcessors();
        public static IMessage GetJsonObjectByJson(string json)
        {

            return
                _jsonsMessagesProcessorsCache
                    .GetJsonObjectByJson(json);
        }
        public static IMessage GetJsonObjectByKey(string key)
        {
            return
                _jsonsMessagesProcessorsCache
                    .GetJsonObjectByKey(key);
        }

        public static void LoadFromImportManyExportsMefParts(IMessage[] messagesParts)
        {
            _jsonsMessagesProcessorsCache
                .LoadFromImportManyExportsMefParts
                    (messagesParts);
        
        }


        public static void LoadFromAssemblies()
        {
            _jsonsMessagesProcessorsCache.LoadFromAssemblies();
        }
    }
}
namespace Microsoft.Boc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;
    using Microsoft.Boc.Share;
    using Newtonsoft.Json.Linq;
    /// <summary>
    /// 类型序列化器缓存
    /// </summary>
    public class JsonsMessagesProcessors
    {
        private Dictionary
                        <
                            string
                            ,
                                Tuple
                                    <
                                         Func<IMessage>        //return IMessage GetJsonObject
                                        , Func<string, IMessage> //json, return IMessage GetJsonObjectByJson
                                    >
                        > _processors = new Dictionary<string, Tuple<Func<IMessage>, Func<string, IMessage>>>();
        public IMessage GetJsonObjectByJson(string json)
        {

            var messageHeader = JsonHelper.DeserializeByJTokenPath<MessageHeader>(json, "H");
            var topic = messageHeader.Topic.ToLower().Trim();
            IMessage r = null;
            Tuple
                <
                     Func<IMessage>        //return IMessage GetJsonObject
                    , Func<string, IMessage> //json, return IMessage GetJsonObjectByJson
                > processFuncs = null;

            if (_processors.TryGetValue(topic, out processFuncs))
            {
                r = processFuncs.Item2(json);
            }
            return r; // serializer;
        }
        public IMessage GetJsonObjectByKey(string key)
        {
            IMessage r = null;
            Tuple
                 <
                      Func<IMessage>        //return IMessage GetJsonObject
                     , Func<string, IMessage> //json, return IMessage GetJsonObjectByJson
                 > processFuncs = null;
            if (_processors.TryGetValue(key, out processFuncs))
            {
                r = processFuncs.Item1();
            }
            return r; // serializer;
        }

        public void LoadFromImportManyExportsMefParts(IMessage[] messagesParts)
        {
            Array
                .ForEach
                    (
                        messagesParts
                        , (x) =>
                        {
                            var key = x.Header.Topic.ToLower().Trim();
                            if (!_processors.ContainsKey(key))
                            {
                                var tuple = Tuple
                                                .Create
                                                    <
                                                        Func<IMessage>
                                                        , Func<string, IMessage>
                                                    >
                                                        (
                                                            x.InstanceGetter
                                                            , x.GetInstanceCreatorByJson()
                                                        );
                                _processors.Add
                                    (
                                        key
                                        , tuple
                                    );
                            }
                        }
                    );
        }

        public void LoadFromAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetLoadableTypes().Where
                (
                    (x) =>
                    {
                        var r = 
                                (
                                    typeof(IMessage).IsAssignableFrom(x)
                                    &&
                                    x != typeof(IMessage)
                                );
                        //if (r)
                        //{
                        //    Console.WriteLine(x.FullName);
                        //}
                        return r;
                    }
                );
                foreach (var type in types)
                {
                    IMessage x = Activator.CreateInstance(type) as IMessage;
                    var key = x.Header.Topic.ToLower().Trim();
                    if (!_processors.ContainsKey(key))
                    {
                        var tuple = Tuple.Create
                                            <
                                                Func<IMessage>
                                                , Func<string, IMessage>
                                            >
                                            (
                                                x.InstanceGetter
                                                , x.GetInstanceCreatorByJson()
                                            );
                        _processors.Add
                            (
                                key
                                , tuple
                            );
                    }
                }
            }
        }
    }
}



//#endif