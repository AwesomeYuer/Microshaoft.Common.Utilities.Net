using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MsTestUnit.Test
{
    [TestClass]
    public class UnitTest1
    {
        private HttpClient _httpClient = new HttpClient();
        [TestMethod]
        public async Task TestMethod1()
        {
            Console.WriteLine("Test1");
            var url = "http://localhost:8100/api/StoreProcedureExecutor/result/mssql/aaa/bbb/objects?top=100&searchobjectname=&searchhostname=&searchdatabasename=&searchaftertime=";
            url = "https://baidu.com";
            var response = await _httpClient.GetAsync(url);
            Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        }
    }
}
