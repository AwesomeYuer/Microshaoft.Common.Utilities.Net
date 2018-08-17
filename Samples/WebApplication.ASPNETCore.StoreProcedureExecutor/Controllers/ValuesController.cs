namespace Microshaoft.AspNETCore.WebApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    [Route("api/[controller]")]
    [ApiController]
    public class valuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        [Route("{sp}")]
        public ActionResult<string> Get
                                (
                                    string sp
                                )
        {
            return null;
        }
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }
        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }
        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }
        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
