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
                            , ICacheAutoRefreshable
    {
        public AbstractStoreProceduresExecutor
                    <SqliteConnection, SqliteCommand, SqliteParameter>
                        _executor = new SqliteStoreProceduresExecutor();
        public string DataBaseType => "sqlite";////this.GetType().Name;
        public int CachedExpiredInSeconds
        {
            get;
            set;
        }
        public bool NeedAutoRefreshForSlideExpire
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
                    CachedExpiredInSeconds > 0
                    &&
                    _executor
                        .CachedExecutingParametersExpiredInSeconds
                    !=
                    CachedExpiredInSeconds
                )
            {
                _executor
                        .CachedExecutingParametersExpiredInSeconds
                            = CachedExpiredInSeconds;
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
            if (NeedAutoRefreshForSlideExpire)
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
