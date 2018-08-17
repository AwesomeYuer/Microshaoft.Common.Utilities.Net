namespace Microshaoft.StoreProcedureExecutors
{
    using Microshaoft;
    using Microsoft.Data.Sqlite;
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using System.Composition;
    using System.Data.Common;

    [Export(typeof(IStoreProcedureExecutable))]
    public class SqliteStoreProcedureExecutorCompositionPlugin
                        : IStoreProcedureExecutable
                            , IStoreProcedureParametersSetCacheAutoRefreshable
    {
        public AbstractStoreProceduresExecutor
                    <SqliteConnection, SqliteCommand, SqliteParameter>
                        _executor = new SqliteStoreProceduresExecutor();
        public string DataBaseType => "sqlite";////this.GetType().Name;
        public int CachedExecutingParametersExpiredInSeconds
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
                        , JToken parameters
                        , out JToken result
                        , int commandTimeoutInSeconds = 90
                    )
        {
            if
                (
                    CachedExecutingParametersExpiredInSeconds > 0
                    &&
                    _executor
                        .CachedExecutingParametersExpiredInSeconds
                    !=
                    CachedExecutingParametersExpiredInSeconds
                )
            {
                _executor
                        .CachedExecutingParametersExpiredInSeconds
                            = CachedExecutingParametersExpiredInSeconds;
            }
            result = null;
            DbConnection connection = new MySqlConnection(connectionString);
            result = _executor
                            .Execute
                                    (
                                        connection
                                        , storeProcedureName
                                        , parameters
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
