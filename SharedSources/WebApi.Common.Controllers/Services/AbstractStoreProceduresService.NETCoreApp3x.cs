#if NETCOREAPP3_X
namespace Microshaoft.Web
{
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Data;

    public abstract partial class
                AbstractStoreProceduresService
                                :
                                    IStoreProceduresWebApiService
                                    , IStoreProceduresService
    {
        public virtual async IAsyncEnumerable
            <
                (
                    int             // resultSetIndex
                    , int           // rowIndex
                    , JArray        // columns
                    , IDataRecord
                )
            >
                    ProcessReaderAsAsyncEnumerable
                (
                    string routeName
                    , JToken parameters = null
                    , string httpMethod = "Get"
                    //, bool enableStatistics = false
                    , int commandTimeoutInSeconds = 101
                )
        {
            //JToken result = null;

            bool success;
            int statusCode;
            string message;
            string connectionString;
            string dataBaseType;
            string storeProcedureName;
            bool enableStatistics;

            (
                success
                , statusCode
                , httpMethod
                , message
                , connectionString
                , dataBaseType
                , storeProcedureName
                , commandTimeoutInSeconds
                , enableStatistics
            )
                = TryGetStoreProcedureInfo
                            (
                                routeName
                                , httpMethod
                            );


            if
               (
                   success
                   &&
                   statusCode == 200
               )
            {
                var entries = ProcessReaderAsAsyncEnumerable
                                (
                                    connectionString
                                    , dataBaseType
                                    , storeProcedureName
                                    , parameters
                                    , enableStatistics
                                    , commandTimeoutInSeconds
                                );
                await foreach (var entry in entries)
                {
                    yield
                        return
                            entry;
                }
            }
        }
        public virtual async IAsyncEnumerable
            <
                (
                    int             // resultSetIndex
                    , int           // rowIndex
                    , JArray        // columns
                    , IDataRecord
                )
            >
                    ProcessReaderAsAsyncEnumerable
                            (
                                string connectionString
                                , string dataBaseType
                                , string storeProcedureName
                                , JToken parameters = null
                                , bool enableStatistics = false
                                , int commandTimeoutInSeconds = 90
                            )
        {
            var success = IndexedExecutors
                                        .TryGetValue
                                            (
                                                dataBaseType
                                                , out var executor
                                            );
            if (success)
            {
                var entries = executor
                                    .ExecuteReaderAsAsyncEnumerable
                                        (
                                            connectionString
                                            , storeProcedureName
                                            , parameters
                                            , enableStatistics
                                            , commandTimeoutInSeconds
                                        );
                await foreach (var entry in entries)
                {
                    yield
                        return
                            entry;
                }
            }
        }
        
    }
}
#endif
