namespace Microshaoft.NET35
{
    using Newtonsoft.Json;
    using System.IO;
    using System.Net;
    using System.Text;
    
    public class HttpClient
    {
        public T Get<T>(string url)
        {
            var httpWebRequest = HttpWebRequest.Create(url);
            byte[] buffer = null;
            using(WebResponse response = httpWebRequest.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                { 
                    buffer = StreamDataHelper.ReadDataToBytes(stream);
                }
            }
            var json = Encoding.UTF8.GetString(buffer);
            T r = JsonConvert.DeserializeObject<T>(json);
            return r;
        }
        public void Post(string url)
        {
            var httpWebRequest = HttpWebRequest.Create(url);
            httpWebRequest.Method = "post";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ContentLength = 0;
            byte[] buffer = null;
            using (WebResponse response = httpWebRequest.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    buffer = StreamDataHelper.ReadDataToBytes(stream);
                }
            }
        }
    }
}

