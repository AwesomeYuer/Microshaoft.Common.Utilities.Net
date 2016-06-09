namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;
    /// <summary>
    /// 类型序列化器缓存
    /// </summary>
    public static class XmlSerializersCache
    {
        private static ConcurrentDictionary<string, XmlSerializer> _data =
                            new ConcurrentDictionary<string, XmlSerializer>();
        public static XmlSerializer GetXmlSerializer(Type type)
        {
            XmlSerializer serializer = null;
            if (!XmlSerializersCache.Data.TryGetValue(type.FullName, out serializer))
            {
                serializer = new XmlSerializer(type);
                XmlSerializersCache.Data.TryAdd(type.FullName, serializer);
            }
            return serializer; // serializer;
        }
        public static XmlSerializer GetXmlSerializer(string typeFullName)
        {
            var type = Type.GetType(typeFullName);
            return GetXmlSerializer(type);
        }
        public static ConcurrentDictionary<string, XmlSerializer> Data
        {
            get { return _data; }
        }
        public static void Load(Func<Type, Assembly, bool> predicateFunc)
        {
            var types = AssemblyHelper
                            .GetAssembliesTypes
                                (
                                    (x, y) =>
                                    {
                                        return predicateFunc(x, y);
                                    }
                                );
            foreach (var type in types)
            {
                _data.TryAdd
                    (
                        type.FullName
                        , new XmlSerializer(type)
                    );
            }
        }
    }
}
