
namespace Microshaoft
{
    using System;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    public class CDATA : IXmlSerializable
    {
        public CDATA()
        {
        }
        public CDATA(string xml)
        {
            this._outerXml = xml;
        }
        private string _outerXml;
        public string OuterXml
        {
            get
            {
                return _outerXml;
            }
        }
        private string _innerXml;
        public string InnerXml
        {
            get
            {
                return _innerXml;
            }
        }
        private string _innerSourceXml;
        public string InnerSourceXml
        {
            get
            {
                return _innerXml;
            }
        }
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            string s = reader.ReadInnerXml();
            string startTag = "<![CDATA[";
            string endTag = "]]>";
            char[] trims = new char[] { '\r', '\n', '\t', ' ' };
            s = s.Trim(trims);
            if (s.StartsWith(startTag) && s.EndsWith(endTag))
            {
                s = s.Substring(startTag.Length, s.LastIndexOf(endTag) - startTag.Length);
            }
            this._innerSourceXml = s;
            this._innerXml = s.Trim(trims);
        }
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteCData(this._outerXml);
        }
    }

}



namespace Test
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Test.Share;
    using Microshaoft;
    public class Class11
    {
        static void Main118(string[] args)
        {
            ServiceBusXmlMessage message = new ServiceBusXmlMessage();
            MessageSecurityHeader security = new MessageSecurityHeader();
            security.SenderID = "sender001";
            security.Signature = "asdasdsadsa";
            security.SignTimeStamp = "asdasdsa";
            Router router = new Router();
            router.Topic = "Topic01";
            router.From = "From01";
            router.FromReferenceID = "11111111111111";
            router.To = new string[] { "to01", "to02", "to03" };
            Encoding e = Encoding.UTF8;
            e = Encoding.GetEncoding("gb2312");
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, e);
            XmlSerializer serializer = new XmlSerializer(router.GetType());
            string xml = SerializerHelper.XmlSerializerObjectToXml<Router>
                                                    (
                                                        router
                                                        , writer
                                                        , serializer
                                                    );
            message.RoutersHeader = new CDATA(xml);
            message.SecurityHeader = security;
            SampleBody sampleBody = new SampleBody();
            sampleBody.TimeStamp = "sadadsad";
            sampleBody.AreaNo = "Area1";
            sampleBody.ChannelNo = "CH1";
            stream = new MemoryStream();
            writer = new XmlTextWriter(stream, e);
            serializer = new XmlSerializer(sampleBody.GetType());
            xml = SerializerHelper.XmlSerializerObjectToXml<SampleBody>
                                                    (
                                                        sampleBody
                                                        , writer
                                                        , serializer
                                                    );
            message.Body = new CDATA(xml);
            stream = new MemoryStream();
            writer = new XmlTextWriter(stream, e);
            serializer = new XmlSerializer(message.GetType());
            xml = SerializerHelper.XmlSerializerObjectToXml<ServiceBusXmlMessage>
                                                    (
                                                        message
                                                        , writer
                                                        , serializer
                                                    );
            Console.WriteLine("Xml序列化:");
            Console.WriteLine(xml);
            Console.WriteLine("Xml反序列化:");
            ServiceBusXmlMessage message2 = SerializerHelper.XmlSerializerXmlToObject<ServiceBusXmlMessage>(xml);
            Console.WriteLine(message2.SecurityHeader.SenderID);
            Console.WriteLine("hi: " + message2.RoutersHeader.InnerSourceXml);
            Console.WriteLine("hi: " + message2.RoutersHeader.InnerXml);
            Console.WriteLine("body: " + message2.Body.OuterXml);
            Console.WriteLine("body: " + message2.Body.InnerXml);
            Router router2 = SerializerHelper.XmlSerializerXmlToObject<Router>(message2.RoutersHeader.InnerXml);
            Console.WriteLine(router2.To[0]);
            SampleBody sampleBody2 = SerializerHelper.XmlSerializerXmlToObject<SampleBody>(message2.Body.InnerXml);
            Console.WriteLine(sampleBody2.AreaNo);
            //Console.WriteLine("Hello World");
            //Console.WriteLine(Environment.Version.ToString());
            Console.ReadLine();
        }
    }
}
namespace Test.Share
{
    using System;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Microshaoft;
    [XmlRoot("ServiceBusXmlMessage")]
    [Serializable]
    public class ServiceBusXmlMessage
    {
        [XmlElement("Security", typeof(MessageSecurityHeader))]
        public MessageSecurityHeader SecurityHeader;
        [XmlElement("Routers", typeof(CDATA))]
        public CDATA RoutersHeader;
        [XmlElement("Body", typeof(CDATA))]
        public CDATA Body;
    }
    [Serializable]
    public class MessageSecurityHeader
    {
        [XmlAttribute("SenderID")]
        public string SenderID;
        [XmlAttribute("Signature")]
        public string Signature;
        [XmlAttribute("SignTimeStamp")]
        public string SignTimeStamp;
    }
    [Serializable]
    public class Router
    {
        [XmlAttribute("Topic")]
        public string Topic;
        [XmlAttribute("From")]
        public string From;
        [XmlAttribute("FromReferenceID")]
        public string FromReferenceID;
        [XmlElement("To", typeof(string))]
        public string[] To;
    }
    [Serializable]
    public class SampleBody
    {
        [XmlAttribute("TimeStamp")]
        public string TimeStamp;
        [XmlElement("AreaNo")]
        public string AreaNo;
        [XmlElement("ChannelNo")]
        public string ChannelNo;
    }
    
}
