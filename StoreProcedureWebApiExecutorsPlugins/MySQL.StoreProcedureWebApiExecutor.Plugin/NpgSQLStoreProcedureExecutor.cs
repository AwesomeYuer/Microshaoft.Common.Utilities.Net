namespace Microshaoft.StoreProcedureExecutors
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using Npgsql;
    using System.Composition;
    using System.Data.Common;

    [Export(typeof(IStoreProcedureExecutable))]
    public class NpgSQLStoreProcedureExecutorCompositionPlugin
                        : IStoreProcedureExecutable
                            , IStoreProcedureParametersSetCacheAutoRefreshable
    {
        public AbstractStoreProceduresExecutor
                    <NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>
                        _executor = new NpgSqlStoreProceduresExecutor();
        public string DataBaseType => "npgsql";////this.GetType().Name;
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
            DbConnection connection = new NpgsqlConnection(connectionString);
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
