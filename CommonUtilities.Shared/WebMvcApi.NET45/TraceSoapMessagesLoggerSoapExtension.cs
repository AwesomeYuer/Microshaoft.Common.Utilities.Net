#if NETFRAMEWORK4_X
namespace Microshaoft
{
    using System;
    using System.IO;
    using System.Web.Services.Protocols;
    using System.Configuration;
    public class TraceSoapMessagesLoggerSoapExtension : SoapExtension
    {
        private Stream _originalStream;
        private Stream _workStream;
        private string _filePath;
        public override Stream ChainStream(Stream stream)
        {
            _originalStream = stream;
            _workStream = new MemoryStream();
            return _workStream;
        }
        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
        {
            //return ((TraceExtensionAttribute)attribute).Filename;
            return null;
        }
        private static string _SoapMessagesLogPath = ConfigurationManager.AppSettings["SoapMessagesLogPath"];
        public override object GetInitializer(Type WebServiceType)
        {
            // Return a file name to log the trace information to, based on the
            // type.
            if (!_SoapMessagesLogPath.EndsWith(@"\"))
            {
                _SoapMessagesLogPath += @"\";
            }
            return _SoapMessagesLogPath + WebServiceType.FullName + ".{0}.{1}.log";
        }
        // Receive the file name stored by GetInitializer and store it in a
        // member variable for this specific instance.
        public override void Initialize(object initializer)
        {
            _filePath = (string)initializer;
        }
        public override void ProcessMessage(SoapMessage message)
        {
            switch (message.Stage)
            {
                case SoapMessageStage.BeforeSerialize:
                    break;
                case SoapMessageStage.AfterSerialize:
                    WriteStream();
                    break;
                case SoapMessageStage.BeforeDeserialize:
                    ReadStream();
                    break;
                case SoapMessageStage.AfterDeserialize:
                    break;
                default:
                    throw new Exception("invalid stage");
            }
        }
        public void WriteStream()
        {
            _workStream.Position = 0;
            byte[] buffer = ReadStreamToBytes(_workStream);
            TextReader reader = new StreamReader(_workStream);
            string s = reader.ReadToEnd();
            WriteLog(s, "response", _filePath);
            _originalStream.Write(buffer, 0, buffer.Length);
        }
        public void ReadStream()
        {
            //解压 请求
            byte[] bytes = ReadStreamToBytes(_originalStream);
            TextReader reader = new StreamReader(_originalStream);
            string s = reader.ReadToEnd();
            WriteLog(s, "request", _filePath);
            _workStream.Write(bytes, 0, bytes.Length);
            _workStream.Position = 0;
        }
        private static void WriteLog(string data, string title, string filePath)
        {
            string path = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fileName = string.Format(filePath, title, DateTime.Now.ToString("yyyy-MM-dd.HH"));
            using 
                    (
                        FileStream fs = new FileStream
                                                (
                                                    fileName
                                                    , FileMode.OpenOrCreate
                                                    , FileAccess.ReadWrite
                                                    , FileShare.ReadWrite
                                                 )
                    )
            {
                StreamWriter w = new StreamWriter(fs);
                w.BaseStream.Seek(0, SeekOrigin.End);
                w.WriteLine(title + "Begin===========" + DateTime.Now);
                w.Flush();
                w.WriteLine(data);
                w.WriteLine(title + "End=============");
                w.Flush();
                w.Close();
            }
        }
        public static byte[] ReadStreamToBytes(Stream stream)
        {
            byte[] buffer = new byte[64 * 1024];
            int r = 0;
            int l = 0;
            long position = -1;
            if (stream.CanSeek)
            {
                position = stream.Position;
                stream.Position = 0;
            }
            MemoryStream ms = new MemoryStream();
            while (true)
            {
                r = stream.Read(buffer, 0, buffer.Length);
                if (r > 0)
                {
                    l += r;
                    ms.Write(buffer, 0, r);
                }
                else
                {
                    break;
                }
            }
            byte[] bytes = new byte[l];
            ms.Position = 0;
            ms.Read(bytes, 0, (int)l);
            ms.Close();
            ms = null;
            if (position >= 0)
            {
                stream.Position = position;
            }
            return bytes;
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class TraceSoapMessagesLoggerSoapExtensionAttribute : SoapExtensionAttribute
    {
        private int _priority;
        public override int Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                _priority = value;
            }
        }
        public override Type ExtensionType
        {
            get
            {
                return typeof(TraceSoapMessagesLoggerSoapExtension);
            }
        }
    }
}
#endif
