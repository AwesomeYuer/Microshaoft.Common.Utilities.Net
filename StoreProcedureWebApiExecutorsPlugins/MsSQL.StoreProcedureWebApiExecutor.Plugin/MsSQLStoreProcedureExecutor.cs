namespace Microshaoft.StoreProcedureExecutors
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Composition;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    [Export(typeof(IStoreProcedureExecutable))]
    public class MsSQLStoreProcedureExecutorCompositionPlugin
                        : IStoreProcedureExecutable
                            , IParametersDefinitionCacheAutoRefreshable
    {
        public AbstractStoreProceduresExecutor
                    <SqlConnection, SqlCommand, SqlParameter>
                        _executor = new MsSqlStoreProceduresExecutor();
        public string DataBaseType => "mssql";////this.GetType().Name;
        public int CachedParametersDefinitionExpiredInSeconds
        {
            get;
            set;
        }
        public bool NeedAutoRefreshExecutedTimeForSlideExpire
        {
            get;
            set;
        }
        public (bool Success, JToken Result) Execute
                    (
                        string connectionString
                        , string storeProcedureName
                        , JToken parameters
                        , Func
                                <
                                    IDataReader
                                    , Type        // fieldType
                                    , string    // fieldName
                                    , int       // row index
                                    , int       // column index
                                    ,
                                        (
                                            bool NeedDefaultProcess
                                            , JProperty Field   //  JObject Field 对象
                                        )
                                > onReadRowColumnProcessFunc = null
                        , bool enableStatistics = false
                        , int commandTimeoutInSeconds = 90
                    )
        {
            (bool Success, JToken Result) r = (Success: false, Result: null);
            SqlConnection connection;
            NewMethod(connectionString, enableStatistics, out connection);
            var result = _executor
                            .Execute
                                    (
                                        connection
                                        , storeProcedureName
                                        , parameters
                                        , onReadRowColumnProcessFunc
                                        , commandTimeoutInSeconds
                                    );
            r = NewMethod1(storeProcedureName, result, connection);
            return r;
        }

        private (bool Success, JToken Result) NewMethod1(string storeProcedureName, JToken result, SqlConnection connection)
        {
            (bool Success, JToken Result) r;
            r.Success = (result != null);
            r.Result = result;
            if (NeedAutoRefreshExecutedTimeForSlideExpire)
            {
                _executor
                    .RefreshCachedExecuted
                        (
                            connection
                            , storeProcedureName
                        );
            }
            return r;
        }

        private void NewMethod(string connectionString, bool enableStatistics, out SqlConnection connection)
        {
            if
                (
                    CachedParametersDefinitionExpiredInSeconds > 0
                    &&
                    _executor
                        .CachedParametersDefinitionExpiredInSeconds
                    !=
                    CachedParametersDefinitionExpiredInSeconds
                )
            {
                _executor
                        .CachedParametersDefinitionExpiredInSeconds
                            = CachedParametersDefinitionExpiredInSeconds;
            }
            connection = new SqlConnection(connectionString);
            if (enableStatistics)
            {
                connection.StatisticsEnabled = enableStatistics;
            }
        }

        public async Task<(bool Success, JToken Result)> 
                    ExecuteAsync
                            (
                                string connectionString
                                , string storeProcedureName
                                , JToken parameters
                                , Func
                                        <
                                            IDataReader
                                            , Type        // fieldType
                                            , string    // fieldName
                                            , int       // row index
                                            , int       // column index
                                            ,
                                                (
                                                    bool NeedDefaultProcess
                                                    , JProperty Field   //  JObject Field 对象
                                                )
                                        > onReadRowColumnProcessFunc = null
                                , bool enableStatistics = false
                                , int commandTimeoutInSeconds = 90
                            )
        {
            (bool Success, JToken Result) r = (Success: false, Result: null);
            SqlConnection connection;
            NewMethod(connectionString, enableStatistics, out connection);
            var result = await _executor
                            .ExecuteAsync
                                    (
                                        connection
                                        , storeProcedureName
                                        , parameters
                                        , onReadRowColumnProcessFunc
                                        , commandTimeoutInSeconds
                                    );
            r = NewMethod1(storeProcedureName, result, connection);
            return r;
        }

       
    }
}
