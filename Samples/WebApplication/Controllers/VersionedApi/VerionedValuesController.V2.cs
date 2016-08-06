
namespace WebApplication.Controllers.Versioned
{
    using Microshaoft;
    using System.Web.Http;
 
    [RoutePrefix("versioned-api/values")]
    //Controller Name 必须 唯一
    public class BController : ApiController
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
        [SemanticVersioned("2.1.10", "2.1.*")]
        public string Get(int id)
        {
            return "getone 2.1.10";
        }
        //[SemanticVersionedRoute("gettwo/{id}", "2.2.10", "2.2.*", typeof(AValues1111111Controller), 2)]
        [Route("gettwo/{id}")]
        [SemanticVersioned("2.2.10", "2.2.*")]
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
