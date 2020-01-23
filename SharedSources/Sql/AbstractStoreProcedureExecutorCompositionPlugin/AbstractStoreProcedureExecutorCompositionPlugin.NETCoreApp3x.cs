#if !NETCOREAPP2_X
namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;

    public abstract partial class
            AbstractStoreProcedureExecutorCompositionPlugin
                        <TDbConnection, TDbCommand, TDbParameter>
                            : IStoreProcedureExecutable
                                , IParametersDefinitionCacheAutoRefreshable
                            where
                                    TDbConnection : DbConnection, new()
                            where
                                    TDbCommand : DbCommand, new()
                            where
                                    TDbParameter : DbParameter, new()
    {
        public async virtual IAsyncEnumerable
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
                        )
        {
            BeforeExecutingProcess
                    (
                        connectionString
                        , enableStatistics
                        , out TDbConnection connection
                    );
            var entries = Executor
                                .ExecuteResultsAsAsyncEnumerable
                                    (
                                        connection
                                        , storeProcedureName
                                        , parameters
                                        , commandTimeoutInSeconds
                                    );
            await
                foreach
                        (
                            var
                                (
                                    resultSetIndex
                                    , rowIndex
                                    , columns
                                    , dataRecord
                                )
                            in
                            entries
                        )
            {
                yield
                    return
                        (
                            resultSetIndex
                            , rowIndex
                            , columns
                            , dataRecord
                        );
            }
            //AfterExecutedProcess
            //    (
            //        storeProcedureName
            //        , connection
            //    );
        }
    }
}
#endif