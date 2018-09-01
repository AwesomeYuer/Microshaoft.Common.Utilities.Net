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
                            , IParametersDefinitionCacheAutoRefreshable
    {
        public AbstractStoreProceduresExecutor
                    <NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>
                        _executor = new NpgSqlStoreProceduresExecutor();
        public string DataBaseType => "npgsql";////this.GetType().Name;
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
                        , JToken parameters
                        , out JToken result
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
