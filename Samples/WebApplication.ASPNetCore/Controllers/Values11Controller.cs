namespace WebApplication.ASPNetCore.Controllers
{
    using Microshaoft;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    [Route("api/[controller]")]
    [ApiController]
    public class StoreProcedureExecutorController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        [Route("{dataSourceName}/{dataBaseName}/{storeProcedureName}")]
        public ActionResult<JObject> Get
                                (
                                    
                                    string dataSourceName
                                    , string databaseName
                                    , string storeProcedureName
                                    ,
                                        [FromQuery]
                                        string p = null //string.Empty
                                )
        {
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(@"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=D:\mssql\MSSQL13.LocalDB\LocalDB\TransportionSecrets\TransportionSecrets.mdf;Data Source=(localdb)\mssqllocaldb;");
                using
                    (
                        SqlCommand command = new SqlCommand(storeProcedureName, connection)
                        {
                            CommandType = CommandType.StoredProcedure
                            , CommandTimeout = 90
                        }
                    )
                {
                    var actualParameters = JObject.Parse(p);
                    var sqlParameters = SqlHelper
                                            .GenerateExecuteSqlParameters
                                                    (
                                                        connection.ConnectionString
                                                        , storeProcedureName
                                                        , actualParameters
                                                    );
                    var parameters = sqlParameters.ToArray();
                    command.Parameters.AddRange(parameters);
                    connection.Open();
                    var dataReader = command.ExecuteReader();
                    var data = dataReader
                                    .AsJTokensEnumerable()
                                    .ToArray();
                    var result = new JObject();
                    var jProperty = new JProperty
                                            (
                                                "ResultSet"
                                                , (object)data
                                            );

                    result.Add(jProperty);
                    var outputParameters
                            = sqlParameters
                                    .Where
                                        (
                                            (x) =>
                                            {
                                                return
                                                    (x.Direction != ParameterDirection.Input);
                                            }
                                        );
                    JObject outputs = null;
                    foreach (var x in outputParameters)
                    {
                        if (outputs == null)
                        {
                            outputs = new JObject();
                        }
                        outputs
                                .Add
                                    (
                                        x.ParameterName.TrimStart('@')
                                        , new JValue(x.Value)
                                    );
                    }
                    if (outputs != null)
                    {
                        result.Add("outputs", outputs);
                    }
                    return result;
                }
            }
            finally
            {
                connection.Close();
                connection = null;
            }
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
