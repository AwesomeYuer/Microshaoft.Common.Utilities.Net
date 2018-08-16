namespace Microshaoft.StoreProcedureExecutors
{
    using Microshaoft;
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using System.Composition;
    [Export(typeof(IStoreProcedureExecutable))]
    public class MySQLStoreProcedureExecutorCompositionPlugin
                        : IStoreProcedureExecutable
                            , IStoreProcedureParametersSetCacheAutoRefreshable
    {
        public MySqlStoreProceduresExecutor _executor = new MySqlStoreProceduresExecutor();

        public string DataBaseType => "mysql";////this.GetType().Name;

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
            MySqlConnection connection = new MySqlConnection(connectionString);
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
