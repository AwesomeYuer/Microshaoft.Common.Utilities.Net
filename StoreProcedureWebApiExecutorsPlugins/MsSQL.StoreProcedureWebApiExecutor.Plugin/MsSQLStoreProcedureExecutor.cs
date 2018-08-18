namespace Microshaoft.StoreProcedureExecutors
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System.Composition;
    using System.Data.Common;
    using System.Data.SqlClient;
    [Export(typeof(IStoreProcedureExecutable))]
    public class MsSQLStoreProcedureExecutorCompositionPlugin
                        : IStoreProcedureExecutable
                            , ICacheAutoRefreshable
    { 
        public AbstractStoreProceduresExecutor
                    <SqlConnection, SqlCommand, SqlParameter>
                        _executor = new MsSqlStoreProceduresExecutor();
        public string DataBaseType => "mssql";////this.GetType().Name;
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
            DbConnection connection = new SqlConnection(connectionString);
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
