
namespace Microshaoft
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Diagnostics;
#if NETFRAMEWORK4_X
    using System.Runtime.Serialization.Formatters.Soap;
#endif
    public static class SerializerHelper
    {
        public static T XmlSerializerXmlToObject<T>(string Xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T Object = XmlSerializerXmlToObject<T>(Xml, serializer);
            return Object;
        }
        public static T XmlSerializerXmlToObject<T>(string Xml, XmlSerializer serializer)
        {
            StringReader stringReader = new StringReader(Xml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            return (T)serializer.Deserialize(xmlReader);
        }
        public static string XmlSerializerObjectToXml<T>
                                    (
                                        T Object
                                        , XmlTextWriter writer
                                        , XmlSerializer serializer
                                    )
        {
            serializer.Serialize(writer, Object);
            MemoryStream stream = writer.BaseStream as MemoryStream;
            byte[] bytes = stream.ToArray();
            Encoding e = EncodingHelper.IdentifyEncoding
                                            (
                                                bytes
                                                , Encoding.GetEncoding("gb2312")
///												, new Encoding[]
///														{
///															Encoding.UTF8
///															, Encoding.Unicode
///														}
                                            );
            byte[] buffer = e.GetPreamble();
            int offset = buffer.Length;
            buffer = new byte[bytes.Length - offset];
            Buffer.BlockCopy(bytes, offset, buffer, 0, buffer.Length);
            string s = e.GetString(buffer);
            return s;
        }
        public static string XmlSerializerObjectToXml<T>(T Object, XmlSerializer serializer)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Encoding e = Encoding.UTF8;
                XmlTextWriter writer = new XmlTextWriter(stream, e);
                string s = XmlSerializerObjectToXml<T>
                                    (
                                        Object
                                        , writer
                                        , serializer
                                    );
                writer.Close();
                writer = null;
                return s;
            }
        }
        public static string XmlSerializerObjectToXml<T>(T Object, Encoding e, XmlSerializer serializer)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlTextWriter writer = new XmlTextWriter(stream, e);
                string s = XmlSerializerObjectToXml<T>
                                    (
                                        Object
                                        , writer
                                        , serializer
                                    );
                writer.Close();
                writer = null;
                return s;
            }
        }
        public static string XmlSerializerObjectToXml<T>(T Object, Encoding e)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                XmlTextWriter writer = new XmlTextWriter(stream, e);
                string s = XmlSerializerObjectToXml<T>
                                    (
                                        Object
                                        , writer
                                        , serializer
                                    );
                writer.Close();
                writer = null;
                return s;
            }

        }
        public static string DataContractSerializerObjectToXml<T>(T Object, DataContractSerializer serializer)
        {
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, Object);
            byte[] buffer = StreamDataHelper.ReadDataToBytes(ms);
            string xml = Encoding.UTF8.GetString(buffer);
            ms.Close();
            ms.Dispose();
            ms = null;
            return xml;
        }
        public static string DataContractSerializerObjectToXml<T>(T Object)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            string xml = DataContractSerializerObjectToXml<T>(Object, serializer);
            return xml;
        }
        public static T DataContractSerializerXmlToObject<T>(string Xml, DataContractSerializer serializer)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(Xml);
            MemoryStream ms = new MemoryStream(buffer);
            //ms.Position = 0;
            T Object = (T)serializer.ReadObject(ms);
            ms.Close();
            ms.Dispose();
            ms = null;
            return Object;
        }
        public static T DataContractSerializerXmlToObject<T>(string Xml)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            byte[] buffer = Encoding.UTF8.GetBytes(Xml);
            MemoryStream ms = new MemoryStream(buffer);
            //ms.Position = 0;
            T Object = (T)serializer.ReadObject(ms);
            ms.Close();
            ms.Dispose();
            ms = null;
            return Object;
        }
#if NETFRAMEWORK4_X
        public static string FormatterObjectToSoap<T>
                             (
                                 T Object
                             )
        {
            using (MemoryStream stream = new MemoryStream())
            {
                SoapFormatter formatter = new SoapFormatter();
                formatter.Serialize(stream, Object);
                string soap = Encoding.UTF8.GetString(stream.GetBuffer());
                return soap;
            }
        }
#endif


#if NETFRAMEWORK4_X
        
        public static T FormatterSoapToObject<T>
                                    (
                                        string soap
                                    )
        {
            using (MemoryStream stream = new MemoryStream())
            {
                SoapFormatter formater = new SoapFormatter();
                byte[] data = Encoding.UTF8.GetBytes(soap);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                T Object = (T)formater.Deserialize(stream);
                return Object;
            }
        }
#endif
        public static byte[] FormatterObjectToBinary<T>
                                    (
                                        T Object
                                    )
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formater = new BinaryFormatter();
                formater.Serialize(stream, Object);
                byte[] buffer = stream.ToArray();
                return buffer;
            }
        }
        public static T FormatterBinaryToObject<T>
                                    (
                                        byte[] data
                                    )
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formater = new BinaryFormatter();
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                T Object = (T)formater.Deserialize(stream);
                return Object;
            }
        }

        public static string DataContractSerializerObjectToJson<T>(T Object)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            string json = DataContractSerializerObjectToJson<T>(Object);
            return json;
        }
        public static string DataContractSerializerObjectToJson<T>(T Object, DataContractJsonSerializer serializer)
        {
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, Object);
            string json = Encoding.UTF8.GetString(ms.GetBuffer());
            ms.Close();
            ms.Dispose();
            ms = null;
            return json;
        }
        public static T DataContractSerializerJsonToObject<T>(string json)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            T Object = DataContractSerializerJsonToObject<T>(json, serializer);
            return Object;
        }
        public static T DataContractSerializerJsonToObject<T>(string json, DataContractJsonSerializer serializer)
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            T Object = (T)serializer.ReadObject(ms);
            ms.Close();
            ms.Dispose();
            ms = null;
            return Object;
        }

    }
}
