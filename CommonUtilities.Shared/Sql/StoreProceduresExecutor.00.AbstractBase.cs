namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    public abstract partial class
            AbstractStoreProceduresExecutor
                    <TDbConnection, TDbCommand, TDbParameter>
                        where
                                TDbConnection : DbConnection, new()
                        where
                                TDbCommand : DbCommand, new()
                        where
                                TDbParameter : DbParameter, new()
    {
        public int CachedParametersDefinitionExpiredInSeconds
        {
            get;
            set;
        }
        protected abstract TDbParameter
                 OnQueryDefinitionsSetInputParameterProcess
                        (
                            TDbParameter parameter
                        );
        protected abstract TDbParameter
                OnQueryDefinitionsSetReturnParameterProcess
                        (
                            TDbParameter parameter
                        );
        protected abstract TDbParameter
                OnQueryDefinitionsReadOneDbParameterProcess
                        (
                            IDataReader reader
                            , TDbParameter parameter
                            , string connectionString
                        );
        protected abstract TDbParameter
                OnExecutingSetDbParameterTypeProcess
                        (
                            TDbParameter definitionSqlParameter
                            , TDbParameter cloneSqlParameter
                        );
        protected abstract object
                OnExecutingSetDbParameterValueProcess
                            (
                                TDbParameter parameter
                                , JToken jValue
                            );
        public List<TDbParameter>
                    GenerateExecuteParameters
                            (
                                string connectionString
                                , string storeProcedureName
                                , JToken inputsParameters
                                , bool includeReturnValueParameter = true
                            )
        {
            List<TDbParameter> result = null;
            var dbParameters
                    = GetCachedParameters
                                (
                                    connectionString
                                    , storeProcedureName
                                    , includeReturnValueParameter
                                );
            var jProperties = (JObject)inputsParameters;
            foreach (KeyValuePair<string, JToken> jProperty in jProperties)
            {
                DbParameter dbParameter = null;
                if
                    (
                        dbParameters
                            .TryGetValue
                                (
                                    jProperty.Key
                                    , out dbParameter
                                )
                    )
                {
                    var direction = dbParameter
                                        .Direction;
                    var dbParameterValue = dbParameter.Value;
                    var includeValueClone = false;
                    if
                        (
                            dbParameterValue != DBNull.Value
                            &&
                            dbParameterValue != null
                        )
                    {
                        DataTable dataTable = dbParameterValue as DataTable;
                        if (dataTable != null)
                        {
                            includeValueClone = true;
                        }
                    }
                    var cloneDbParameter
                            = dbParameter
                                    .ShallowClone
                                        <TDbParameter>
                                            (
                                                OnExecutingSetDbParameterTypeProcess
                                                , includeValueClone
                                            );
                    var parameterValue =
                            OnExecutingSetDbParameterValueProcess
                                    (
                                        cloneDbParameter
                                        , jProperty.Value
                                    );
                    cloneDbParameter.Value = parameterValue;
                    if (result == null)
                    {
                        result = new List<TDbParameter>();
                    }
                    result
                        .Add(cloneDbParameter);
                }
            }
            foreach (var kvp in dbParameters)
            {
                var dbParameter = kvp.Value;
                if (result == null)
                {
                    result = new List<TDbParameter>();
                }
                if
                    (
                        !result
                            .Exists
                                (
                                    (x) =>
                                    {
                                        return
                                            (
                                                string
                                                    .Compare
                                                        (
                                                            x.ParameterName
                                                            , dbParameter
                                                                    .ParameterName
                                                            , true
                                                        ) == 0
                                            );
                                    }
                                )
                    )
                {
                    var direction = dbParameter.Direction;
                    if
                        (
                            direction != ParameterDirection.Input
                        )
                    {
                        if (result == null)
                        {
                            result = new List<TDbParameter>();
                        }
                        var cloneDbParameter =
                                dbParameter
                                    .ShallowClone
                                        <TDbParameter>
                                            (
                                                OnExecutingSetDbParameterTypeProcess
                                            );
                        //if (direction == ParameterDirection.InputOutput)
                        //{
                        //    cloneDbParameter.Direction = ParameterDirection.Output;
                        //}
                        result.Add(cloneDbParameter);
                    }
                }
            }
            return result;
        }
        private class ExecutingInfo
        {
            public IDictionary<string, DbParameter> DbParameters;
            public DateTime RecentParametersDefinitionCacheUsedTime;
            public object Locker = new object();
        }
        private
            ConcurrentDictionary<string, ExecutingInfo>
                _dictionary
                    = new ConcurrentDictionary<string, ExecutingInfo>
                            (
                                StringComparer
                                        .OrdinalIgnoreCase
                            );
        public void RefreshCachedExecuted
                            (
                                DbConnection connection
                                , string storeProcedureName
                            )
        {
            var dataSource = connection.DataSource;
            var dataBase = connection.Database;
            var key = $"{connection.DataSource}-{connection.Database}-{storeProcedureName}".ToUpper();
            ExecutingInfo executingInfo = null;
            if
                (
                    _dictionary
                        .TryGetValue
                            (
                                key
                                , out executingInfo
                            )
                )
            {
                executingInfo
                    .RecentParametersDefinitionCacheUsedTime = DateTime.Now;
            }
        }
        public IDictionary<string, DbParameter>
                    GetCachedParameters
                                (
                                    string connectionString
                                    , string storeProcedureName
                                    , bool includeReturnValueParameter = false
                                )
        {
            ExecutingInfo GetExecutingInfo()
            {
                var nameIndexedParameters =
                            GetNameIndexedDefinitionParameters
                                    (
                                        connectionString
                                        , storeProcedureName
                                        , includeReturnValueParameter
                                    );
                var _executingInfo = new ExecutingInfo()
                {
                    DbParameters = nameIndexedParameters
                    , RecentParametersDefinitionCacheUsedTime = DateTime.Now
                };
                return _executingInfo;
            }
            DbConnection connection = new TDbConnection();
            connection.ConnectionString = connectionString;
            var key = $"{connection.DataSource}-{connection.Database}-{storeProcedureName}".ToUpper();
            var add = false;
            var executingInfo = 
                                _dictionary
                                        .GetOrAdd
                                                (
                                                    key
                                                    , (x) =>
                                                    {
                                                        var r = GetExecutingInfo();
                                                        add = true;
                                                        return r;
                                                    }
                                                );
            var result = executingInfo.DbParameters;
            if (!add)
            {
                if (CachedParametersDefinitionExpiredInSeconds > 0)
                {
                    var locker = executingInfo
                                            .Locker;
                    locker
                        .LockIf
                            (
                                () =>
                                {
                                    var diffSeconds = DateTimeHelper
                                                            .SecondsDiffNow
                                                                (
                                                                    executingInfo
                                                                            .RecentParametersDefinitionCacheUsedTime
                                                                );
                                    var r =
                                            (
                                                diffSeconds
                                                >
                                                CachedParametersDefinitionExpiredInSeconds
                                            );
                                    return r;
                                }
                                ,
                                () =>
                                {
                                    executingInfo
                                        .DbParameters =
                                                GetNameIndexedDefinitionParameters
                                                    (
                                                        connectionString
                                                        , storeProcedureName
                                                        , includeReturnValueParameter
                                                    );
                                    executingInfo
                                        .RecentParametersDefinitionCacheUsedTime = DateTime.Now;
                                }
                            );
                }
            }
            return result;
        }
        public IEnumerable<TDbParameter>
                                GetDefinitionParameters
                                        (
                                            string connectionString
                                            , string storeProcedureName
                                            , bool includeReturnValueParameter = false
                                        )
        {
            var r = SqlHelper
                        .GetStoreProcedureDefinitionParameters
                                <TDbConnection, TDbCommand, TDbParameter>
                                    (
                                        connectionString
                                        , storeProcedureName
                                        , OnQueryDefinitionsSetInputParameterProcess
                                        , OnQueryDefinitionsSetReturnParameterProcess
                                        , OnQueryDefinitionsReadOneDbParameterProcess
                                        , includeReturnValueParameter
                                    );
            return r;
        }
        public IDictionary<string, DbParameter>
                        GetNameIndexedDefinitionParameters
                                (
                                    string connectionString
                                    , string storeProcedureName
                                    , bool includeReturnValueParameter = false
                                )

        {
            var dbParameters =
                            GetDefinitionParameters
                                (
                                    connectionString
                                    , storeProcedureName
                                    , includeReturnValueParameter
                                );
            var result =
                    dbParameters
                        .ToDictionary
                            (
                                (xx) =>
                                {
                                    return
                                        xx
                                            .ParameterName
                                            .TrimStart('@', '?');
                                }
                                , (xx) =>
                                {
                                    return
                                        (DbParameter)xx;
                                }
                                , StringComparer
                                        .OrdinalIgnoreCase
                            );
            return result;
        }
        public ParameterDirection GetParameterDirection(string parameterMode)
        {
            var r = SqlHelper
                            .GetParameterDirection
                                (parameterMode);
            return r;
        }
        //public JToken
        //            Execute
        //                (
        //                    DbConnection connection
        //                    , string storeProcedureName
        //                    , string p = null //string.Empty
        //                    , Func
        //                        <
        //                            IDataReader
        //                            , Type        // fieldType
        //                            , string    // fieldName
        //                            , int       // row index
        //                            , int       // column index
        //                            ,
        //                                (
        //                                    bool needDefaultProcess
        //                                    , JProperty field   //  JObject Field 对象
        //                                )
        //                        > onReadRowColumnProcessFunc = null
        //                    , int commandTimeout = 90
        //                )
        //{
        //    var inputsParameters = JToken.Parse(p);
        //    return
        //        Execute
        //            (
        //                connection
        //                , storeProcedureName
        //                , inputsParameters
        //                , onReadRowColumnProcessFunc
        //                , commandTimeout
        //            );
        //}

        public JToken
                    Execute111111111
                        (
                            TDbConnection connection
                            , string storeProcedureName
                            , JToken inputsParameters = null //string.Empty
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
                            //, bool enableStatistics = false
                            , int commandTimeoutInSeconds = 90
                        )
        {
            var dataSource = connection.DataSource;
            var dataBaseName = connection.Database;
            SqlConnection sqlConnection = connection as SqlConnection;
            var isSqlConnection = (sqlConnection != null);
            var statisticsEnabled = false;
            if (isSqlConnection)
            {
                statisticsEnabled = sqlConnection.StatisticsEnabled;
            }
            SqlCommand sqlCommand = null;
            StatementCompletedEventHandler onStatementCompletedEventHandlerProcessAction = null;
            SqlInfoMessageEventHandler onSqlInfoMessageEventHandlerProcessAction = null;
            try
            {
                using
                    (
                        TDbCommand command = new TDbCommand()
                        {
                            CommandType = CommandType.StoredProcedure
                            , CommandTimeout = commandTimeoutInSeconds
                            , CommandText = storeProcedureName
                            , Connection = connection
                        }
                    )
                {

                    if (commandTimeoutInSeconds > 0)
                    {
                        command.CommandTimeout = commandTimeoutInSeconds;
                    }
                    var dbParameters
                            = GenerateExecuteParameters
                                    (
                                        connection.ConnectionString
                                        , storeProcedureName
                                        , inputsParameters
                                    );
                    if (dbParameters != null)
                    {
                        var parameters = dbParameters.ToArray();
                        command
                            .Parameters
                            .AddRange(parameters);
                    }
                    connection.Open();
                    var result = new JObject
                    {
                        {
                            "BeginTime"
                            , null
                        }
                        ,
                        {
                            "EndTime"
                            , null
                        }
                        ,
                        {
                            "DurationInMilliseconds"
                            , null
                        }
                        ,
                        {
                            "Outputs"
                            , new JObject
                                {
                                    {
                                        "Parameters"
                                            , null
                                    }
                                    ,
                                    {
                                        "ResultSets"
                                            , new JArray()
                                    }
                                }
                        }
                    };
                    var dataReader = command
                                        .ExecuteReader
                                            (
                                                CommandBehavior
                                                    .CloseConnection
                                            );

                    int resultSetID = 0;
                    int messageID = 0;
                    JArray recordCounts = null;
                    JArray messages = null;
                    if (statisticsEnabled)
                    {
                        recordCounts = new JArray();
                        messages = new JArray();
                        sqlCommand = command as SqlCommand;
                        onStatementCompletedEventHandlerProcessAction =
                                (sender, statementCompletedEventArgs) =>
                                {
                                    recordCounts.Add(statementCompletedEventArgs.RecordCount);
                                };
                        sqlCommand.StatementCompleted += onStatementCompletedEventHandlerProcessAction;
                        onSqlInfoMessageEventHandlerProcessAction =
                        (sender, sqlInfoMessageEventArgs) =>
                        {
                            messageID++;
                            messages
                                    .Add
                                        (
                                            new JObject()
                                            {
                                                {
                                                    "MessageID"
                                                    , messageID
                                                }
                                                ,
                                                {
                                                    "ResultSetID"
                                                    , resultSetID
                                                }
                                                ,
                                                {
                                                    "Source"
                                                    , sqlInfoMessageEventArgs.Source
                                                }
                                                ,
                                                {
                                                    "Message"
                                                    , sqlInfoMessageEventArgs.Message
                                                }
                                                ,
                                                {
                                                    "DealTime"
                                                    , DateTime.Now
                                                }
                                            }
                                        );
                        };
                        sqlConnection.InfoMessage += onSqlInfoMessageEventHandlerProcessAction;
                    }
                    do
                    {
                        var columns = dataReader
                                        .GetColumnsJArray();
                        var rows = dataReader
                                        .AsRowsJTokensEnumerable
                                            (
                                                columns
                                                , onReadRowColumnProcessFunc
                                            );
                        var resultSet = new JObject
                                {
                                    {
                                        "Columns"
                                        , columns
                                    }
                                    ,
                                    {
                                        "Rows"
                                        , new JArray(rows)
                                    }
                                };
                        (
                            (JArray)
                                result
                                    ["Outputs"]
                                    ["ResultSets"]
                        )
                        .Add
                            (
                                resultSet
                            );
                        resultSetID++;
                    }
                    while (dataReader.NextResult());
                    dataReader.Close();
                    JObject jOutputParameters = null;
                    if (dbParameters != null)
                    {
                        var outputParameters =
                                    dbParameters
                                            .Where
                                                (
                                                    (x) =>
                                                    {
                                                        return
                                                            (
                                                                x
                                                                    .Direction
                                                                !=
                                                                ParameterDirection
                                                                    .Input
                                                            );
                                                    }
                                                );
                        foreach (var x in outputParameters)
                        {
                            if (jOutputParameters == null)
                            {
                                jOutputParameters = new JObject();
                            }
                            jOutputParameters
                                    .Add
                                        (
                                            x.ParameterName.TrimStart('@', '?')
                                            , new JValue(x.Value)
                                        );
                        }
                    }
                    if (jOutputParameters != null)
                    {
                        result["Outputs"]["Parameters"] = jOutputParameters;
                    }
                    if (statisticsEnabled)
                    {
                        //if (sqlConnection.StatisticsEnabled)
                        {
                            var j = new JObject();
                            var statistics = sqlConnection.RetrieveStatistics();
                            var json = JsonHelper.Serialize(statistics);
                            var jStatistics = JObject.Parse(json);
                            var jCurrent = result["DurationInMilliseconds"];
                            jCurrent
                                .Parent
                                .AddAfterSelf
                                    (
                                        new JProperty
                                                (
                                                    "DataBaseStatistics"
                                                    , jStatistics
                                                )
                                    );
                            if (messages != null)
                            {
                                result["DataBaseStatistics"]["Messages"] = messages;
                            }
                            if (recordCounts != null)
                            {
                                jCurrent
                                    .Parent
                                    .AddAfterSelf
                                            (
                                                new JProperty
                                                        (
                                                            "RecordCounts"
                                                            , recordCounts
                                                        )
                                            );
                            }
                        }
                        if (onStatementCompletedEventHandlerProcessAction != null)
                        {
                            sqlCommand.StatementCompleted -= onStatementCompletedEventHandlerProcessAction;
                            onStatementCompletedEventHandlerProcessAction = null;
                        }
                        if (onSqlInfoMessageEventHandlerProcessAction != null)
                        {
                            sqlConnection.InfoMessage -= onSqlInfoMessageEventHandlerProcessAction;
                            onSqlInfoMessageEventHandlerProcessAction = null;
                        }
                    }
                    return result;
                }
            }
            finally
            {
                if (isSqlConnection)
                {
                    if (onStatementCompletedEventHandlerProcessAction != null)
                    {
                        sqlCommand.StatementCompleted -= onStatementCompletedEventHandlerProcessAction;
                        onStatementCompletedEventHandlerProcessAction = null;
                    }
                    if (onSqlInfoMessageEventHandlerProcessAction != null)
                    {
                        sqlConnection.InfoMessage -= onSqlInfoMessageEventHandlerProcessAction;
                        onSqlInfoMessageEventHandlerProcessAction = null;
                    }
                    if (sqlConnection.StatisticsEnabled)
                    {
                        sqlConnection.StatisticsEnabled = false;
                    }
                    sqlConnection = null;
                }
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
                connection = null;
            }
        }
    }
}