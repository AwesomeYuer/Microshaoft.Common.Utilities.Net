#if NETCOREAPP3_X
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Data;

    public partial interface IStoreProcedureExecutable
    {
        IAsyncEnumerable
                        <
                            (
                                int             // resultSetIndex
                                , int           // rowIndex
                                , JArray        // columns
                                , IDataRecord
                            )
                        >
                ExecuteReaderAsAsyncEnumerable
                        (
                            string connectionString
                            , string storeProcedureName
                            , JToken parameters = null
                            , bool enableStatistics = false
                            , int commandTimeoutInSeconds = 90
                        );
    }
}
#endif