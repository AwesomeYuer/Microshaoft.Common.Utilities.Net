using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject
{
    public class UnitTest1
    {
        private HttpClient _httpClient = new HttpClient();
        [Fact]
        public async Task Test1()
        {
            Console.WriteLine("Test1");
            var url = "http://localhost:8100/api/StoreProcedureExecutor/result/mssql/aaa/bbb/objects?top=100&searchobjectname=&searchhostname=&searchdatabasename=&searchaftertime=";
            url = "https://baidu.com";
            var response = await _httpClient.GetAsync(url);
            Assert.Equal<HttpStatusCode>(HttpStatusCode.Found, response.StatusCode);
        }
    }
}
