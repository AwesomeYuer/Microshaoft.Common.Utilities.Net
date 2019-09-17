
namespace Microshaoft
{
    #region Using directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    #endregion

    public static class FtpWebRequestResponseHelper
    {
        static void Main111(string[] args)
        {
            string url = "ftp://ftp.microshaoft.com/NDP40-KB2468871-v2-x64.zip";
            string fileName = Path.GetFileName((new Uri(url)).AbsolutePath);
            using (Stream stream = File.OpenWrite(@"d:\temp4\FtpDownLoad\" + fileName))
            {
                FtpWebRequestResponseHelper.Download
                                            (
                                                "ftpuser"
                                                , "pass01!"
                                                , url
                                                , 1024
                                                , null
                    //(buffer, r, l) =>
                    //{
                    //    stream.Write(buffer, 0, r);
                    //    return false;
                    //}
                                            );
            }
            url = "ftp://ftp.microshaoft.com";
            List<string> list = new List<string>();
            using (Stream stream = new MemoryStream())
            {
                FtpWebRequestResponseHelper.SendFtpMethod
                                            (
                                                "ftpuser"
                                                , "pass01!"
                                                , url
                                                , 1024
                                                , WebRequestMethods.Ftp.ListDirectory
                                                , null
                                                , (s, i) =>
                                                {

                                                    s = Path.GetFileName(s);
                                                    Console.WriteLine(s);
                                                    list.Add(s);
                                                    return false;
                                                }
                                            );


            }



            //Parallel.ForEach
            //            (
            //                list
            //                , (item) =>
            //                {
            //                    FtpWebRequestResponseHelper.Download

            //                }
            //            );
            Console.ReadLine();


        }

        public static bool Upload
                               (
                                   string user
                                   , string password
                                   , string uploadToUrl
                                   , string localFileFullName
                                   , string uploadingFileExtensionName
                                   , int bufferLength
                               )
        {
            bool b;
            string fileName = Path.GetFileName(localFileFullName);
            string url = string.Format("{1}{0}{2}{3}", "", uploadToUrl, fileName, uploadingFileExtensionName);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(user, password);
            byte[] buffer = new byte[bufferLength];
            long l;
            using (FileStream fs = File.OpenRead(localFileFullName))
            {
                using (Stream stream = request.GetRequestStream())
                {
                    int r;
                    l = fs.Length;
                    long p = 0;
                    while (p < l)
                    {
                        r = fs.Read(buffer, 0, buffer.Length);
                        stream.Write(buffer, 0, r);
                        p += r;
                    }

                    stream.Close();
                    fs.Close();
                }
            }
            FtpWebResponse response = null;
            //try
            //{
            response = (FtpWebResponse)request.GetResponse();

            //}
            //catch (WebException we)
            //{
            //    Console.WriteLine(we.ToString());
            //    throw new Exception("FtpWebRequestResponseHelper", we);
            //    //return false;
            //}
            long ll = FtpWebRequestResponseHelper.GetFileSize(user, password, url);
            b = (ll == l);

            if (b)
            {
                FtpWebRequestResponseHelper.SendFtpMethod
                                                (
                                                    user
                                                    , password
                                                    , url
                                                    , bufferLength
                                                    , WebRequestMethods.Ftp.Rename
                                                    , (x) =>
                                                    {
                                                        x.RenameTo = fileName;
                                                        return false;
                                                    }
                                                    , (s, i) =>
                                                    {
                                                        return false;
                                                    }

                                                );
                b = true;

            }
            else
            {
                FtpWebRequestResponseHelper.DeleteFile
                                                (
                                                    user
                                                    , password
                                                    , url
                                                );
                b = false;

            }



            return b;

        }

        public static bool DeleteFile
                                (
                                    string user
                                    , string password
                                    , string url
                                )
        {
            bool r;
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential(user, password);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            r = true;
            return r;

        }

        public static long GetFileSize
                                (
                                    string user
                                    , string password
                                    , string url
                                )
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = new NetworkCredential(user, password);
            FtpWebResponse response = null;
            //try
            //{
            response = (FtpWebResponse)request.GetResponse();

            //}
            //catch (WebException we)
            //{
            //    Console.WriteLine(we.ToString());
            //    throw new Exception("FtpWebRequestResponseHelper", we);
            //    return -1;
            //}
            long l = response.ContentLength;
            return l;
        }

        public static void SendFtpMethod
                                (
                                    string user
                                    , string password
                                    , string url
                                    , int bufferLength
                                    , string method
                                    , Func<FtpWebRequest, bool> processRequestFunc
                                    , Func<string, int, bool> processResponseReadLineFunc
                                )
        {
            if (processResponseReadLineFunc != null)
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = method;
                if (processRequestFunc != null)
                {
                    if (processRequestFunc(request))
                    {
                        return;
                    }

                }
                request.Credentials = new NetworkCredential(user, password);
                FtpWebResponse response = null;
                //try
                //{
                response = (FtpWebResponse)request.GetResponse();

                //}
                //catch (WebException we)
                //{
                //    Console.WriteLine(we.ToString());
                //    throw new Exception("FtpWebRequestResponseHelper", we);
                //    //return;
                //}
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    long l = response.ContentLength;
                    //string fileName = Path.GetFileName(request.RequestUri.AbsolutePath);
                    byte[] buffer = new byte[bufferLength];
                    int i = 0;
                    while (!sr.EndOfStream) //(p < l)
                    {
                        string s = sr.ReadLine();
                        if (processResponseReadLineFunc(s, i))
                        {
                            break;
                        }
                        i++;
                    }
                }


            }



        }



        public static void Download
                                (
                                    string user
                                    , string password
                                    , string url
                                    , int bufferLength
                                    , Func<byte[], int, int, bool> processBufferBlockFunc
                                )
        {
            if (processBufferBlockFunc != null)
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(user, password);
                FtpWebResponse response = null;
                //try
                //{    
                response = (FtpWebResponse)request.GetResponse();

                //}
                //catch (WebException we)
                //{
                //    Console.WriteLine(we.ToString());
                //    throw new Exception("FtpWebRequestResponseHelper", we);
                //    //return;
                //}
                using (Stream stream = response.GetResponseStream())
                {
                    long l = response.ContentLength;
                    //string fileName = Path.GetFileName(request.RequestUri.AbsolutePath);
                    byte[] buffer = new byte[bufferLength];
                    int p = 0;
                    while (true) //(p < l)
                    {
                        int r = stream.Read(buffer, 0, buffer.Length);
                        if (r > 0)
                        {
                            if (processBufferBlockFunc(buffer, r, p))
                            {
                                break;
                            }
                            p += r;
                        }
                        else
                        {
                            break;
                        }

                    }
                }

            }
        }

    }
}
