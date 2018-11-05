namespace Microshaoft.StoreProcedureExecutors
{
    using Microshaoft;
    using Microsoft.Data.Sqlite;
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Composition;
    using System.Data;
    using System.Data.Common;

    [Export(typeof(IStoreProcedureExecutable))]
    public class SqliteStoreProcedureExecutorCompositionPlugin
                        : IStoreProcedureExecutable
                            , IParametersDefinitionCacheAutoRefreshable
    {
        public AbstractStoreProceduresExecutor
                    <SqliteConnection, SqliteCommand, SqliteParameter>
                        _executor = new SqliteStoreProceduresExecutor();
        public string DataBaseType => "sqlite";////this.GetType().Name;
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
        public bool Execute
                    (
                        string connectionString
                        , string storeProcedureName
                        , out JToken result
                        , JToken parameters = null
                        , Func
                            <
                                IDataReader
                                , Type        // fieldType
                                , string    // fieldName
                                , int       // row index
                                , int       // column index
                                , JProperty   //  JObject Field 对象
                            > onReadRowColumnProcessFunc = null
                        , bool enableStatistics = false
                        , int commandTimeoutInSeconds = 90
                    )
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
            result = null;
            DbConnection connection = new MySqlConnection(connectionString);
            result = _executor
                            .Execute
                                    (
                                        connection
                                        , storeProcedureName
                                        , parameters
                                        , onReadRowColumnProcessFunc
                                        , commandTimeoutInSeconds
                                    );
            if (NeedAutoRefreshExecutedTimeForSlideExpire)
            {
                _executor
                    .RefreshCachedExecuted
                        (
                            connection
                            , storeProcedureName
                        );
            }
            return true;
        }
    }
}
