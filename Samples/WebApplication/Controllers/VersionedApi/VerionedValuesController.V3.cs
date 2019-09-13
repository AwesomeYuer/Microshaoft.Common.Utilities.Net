
namespace WebApplication.Controllers.Versioned
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Microshaoft.Web;
    using Microshaoft;
    //[Authorize]

    [RoutePrefix("versioned-api/values")]
    //[WebApiVersion("1.0.0.11")]
    public class CController : ApiController
    {
        // GET api/values
        //[Route]

        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/values/5
        //[Route("getone/{id}")]
        //[VersionedRoute("getone/{id}", 2)]
        //[SemanticVersionedRoute("getone/{id}", "2.1.10", "2.1.*", typeof(AValues1111111Controller), 2)]
        [Route("getone/{id}")]
        [SemanticVersioned("3.1.10", "3.1.*")]
        public string Get(int id)
        {
            return "getone 3.1.10";
        }
        //[SemanticVersionedRoute("gettwo/{id}", "2.2.10", "2.2.*", typeof(AValues1111111Controller), 2)]
        [Route("gettwo/{id}")]
        [SemanticVersioned("3.2.10", "3.2.*")]
        public string Get2(int id)
        {
            return "gettwo 2.2.10";
        }
        // POST api/values
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/values/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //public void Delete(int id)
        //{
        //}
    }
}
