
namespace WebApplication.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Microshaoft.Web;
    using Microshaoft.WebApi.Versioning;
    //[Authorize]

    [RoutePrefix("api/values")]
    //[WebApiVersion("1.0.0.10")]

    public class BValues777777Controller : ApiController
    {
        // GET api/values
        //[Route]
        //[VersionedRoute]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        //[Route("getone/{id}")]
        //[VersionedRoute("getone/{id}", 1)]
        [SemanticVersionedRoute("getone/{id}","1.1.10", "1.1.*",typeof(BValues777777Controller),1)]
        public string Get(int id)
        {
            return "getone 1.1.10";
        }

        [SemanticVersionedRoute("gettwo/{id}", "1.2.10", "1.2.*", typeof(BValues777777Controller), 1)]
        public string Get2(int id)
        {
            return "gettwo 1.2.10";
        }


        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
