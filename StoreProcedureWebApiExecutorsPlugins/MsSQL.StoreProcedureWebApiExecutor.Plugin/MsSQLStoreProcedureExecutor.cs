namespace Microshaoft.StoreProcedureExecutors
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Composition;
    using System.Data;
    using System.Data.SqlClient;
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
        public bool Execute
                    (
                        string connectionString
                        , string storeProcedureName
                        , out JToken result
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
            var connection = new SqlConnection(connectionString);
            JArray messages = null;
            SqlInfoMessageEventHandler onSqlInfoMessageProcessAction = null;
            if (enableStatistics)
            {
                connection.StatisticsEnabled = enableStatistics;
                messages = new JArray();
                onSqlInfoMessageProcessAction =
                        (sender, sqlInfoMessageEventArgs) =>
                        {
                            messages
                                .Add
                                    (
                                        new JObject()
                                        {
                                            {
                                                "Source"
                                                , sqlInfoMessageEventArgs.Source
                                            }
                                            ,
                                            {
                                                "Message"
                                                , sqlInfoMessageEventArgs.Message
                                            }
                                        }
                                    );
                    
                        };
                connection.InfoMessage += onSqlInfoMessageProcessAction;
            }
            result = _executor
                            .Execute
                                    (
                                        connection
                                        , storeProcedureName
                                        , parameters
                                        , onReadRowColumnProcessFunc
                                        , commandTimeoutInSeconds
                                    );
            if (messages != null)
            {
                result["DataBaseStatistics"]["Messages"] = messages;
            }
            if (onSqlInfoMessageProcessAction != null)
            {
                connection.InfoMessage -= onSqlInfoMessageProcessAction;
            }
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
