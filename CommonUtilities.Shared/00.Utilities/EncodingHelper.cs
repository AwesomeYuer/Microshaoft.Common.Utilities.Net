namespace Microshaoft
{
    using System.IO;
    using System.Text;
    using System.Collections.Generic;
    public static class EncodingHelper
    {
        public static Encoding IdentifyEncoding
                                    (
                                        Stream stream
                                        , Encoding defaultEncoding
                                        , Encoding[] identifyEncodings
                                    )
        {
            byte[] data = StreamDataHelper.ReadDataToBytes(stream);
            return IdentifyEncoding
                        (
                            data
                            , defaultEncoding
                            , identifyEncodings
                        );
        }
        public static Encoding IdentifyEncoding
                                    (
                                        Stream stream
                                        , Encoding defaultEncoding
                                    )
        {
            byte[] data = StreamDataHelper.ReadDataToBytes(stream);
            return IdentifyEncoding
                        (
                            data
                            , defaultEncoding
                        );
        }
        public static Encoding IdentifyEncoding
                                    (
                                        byte[] data
                                        , Encoding defaultEncoding
                                    )
        {
            EncodingInfo[] encodingInfos = Encoding.GetEncodings();
            List<Encoding> list = new List<Encoding>();
            foreach (EncodingInfo info in encodingInfos)
            {
                Encoding e = info.GetEncoding();
                if (e.GetPreamble().Length > 0)
                {
                    list.Add(e);
                    //System.Console.WriteLine(e.EncodingName);
                }
            }
            Encoding[] encodings = new Encoding[list.Count];
            list.CopyTo(encodings);
            return IdentifyEncoding
                        (
                            data
                            , defaultEncoding
                            , encodings
                        );
        }
        public static Encoding IdentifyEncoding
                                    (
                                        byte[] data
                                        , Encoding defaultEncoding
                                        , Encoding[] identifyEncodings
                                    )
        {
            Encoding encoding = defaultEncoding;
            foreach (Encoding e in identifyEncodings)
            {
                byte[] buffer = e.GetPreamble();
                int l = buffer.Length;
                if (l == 0)
                {
                    continue;
                }
                bool flag = false;
                for (int i = 0; i < l; i++)
                {
                    if (buffer[i] != data[i])
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    continue;
                }
                else
                {
                    encoding = e;
                }
            }
            return encoding;
        }
    }
}