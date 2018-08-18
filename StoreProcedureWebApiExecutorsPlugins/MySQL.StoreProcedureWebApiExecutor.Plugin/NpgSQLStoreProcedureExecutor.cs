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
                            , ICacheAutoRefreshable
    {
        public AbstractStoreProceduresExecutor
                    <NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>
                        _executor = new NpgSqlStoreProceduresExecutor();
        public string DataBaseType => "npgsql";////this.GetType().Name;
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
            DbConnection connection = new NpgsqlConnection(connectionString);
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
