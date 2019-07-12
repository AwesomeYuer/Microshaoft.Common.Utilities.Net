namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

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

        private class ExtensionInfo
        {
            public int resultSetID = 0;
            public int messageID = 0;
            public JArray recordCounts = null;
            public JArray messages = null;
            public void Clear()
            {
                recordCounts = null;
                messages = null;
            }
        }

        private static void DataReadingProcess
                        (
                            Func
                                <
                                    IDataReader
                                    , Type
                                    , string
                                    , int
                                    , int
                                    ,
                                        (
                                            bool NeedDefaultProcess
                                            , JProperty Field
                                        )
                                > onReadRowColumnProcessFunc
                            , JObject result
                            , DbDataReader dataReader
                        )
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
        }

        private static void ResultProcess
                            (
                                TDbConnection connection
                                , bool statisticsEnabled
                                , TDbCommand command
                                , ref StatementCompletedEventHandler
                                            onStatementCompletedEventHandlerProcessAction
                                , ref SqlInfoMessageEventHandler
                                            onSqlInfoMessageEventHandlerProcessAction
                                , List<TDbParameter> dbParameters
                                , JObject result
                                , ExtensionInfo extensionInfo
                            )
        {
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
                                x
                                    .ParameterName
                                    .TrimStart('@', '?')
                                , new JValue(x.Value)
                            );
                }
            }
            if (jOutputParameters != null)
            {
                result
                    ["Outputs"]
                    ["Parameters"] = jOutputParameters;
            }
            // MSSQL 专用
            if (statisticsEnabled)
            {
                var j = new JObject();
                var jCurrent = result["DurationInMilliseconds"];
                if (connection is SqlConnection sqlConnection)
                {
                    var statistics = sqlConnection.RetrieveStatistics();
                    var json = JsonHelper.Serialize(statistics);
                    var jStatistics = JObject.Parse(json);
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
                    if (extensionInfo.messages != null)
                    {
                        result
                            ["DataBaseStatistics"]
                            ["Messages"] = extensionInfo.messages;
                    }
                    if (extensionInfo.recordCounts != null)
                    {
                        jCurrent
                            .Parent
                            .AddAfterSelf
                                (
                                    new JProperty
                                            (
                                                "RecordCounts"
                                                , extensionInfo
                                                        .recordCounts
                                            )
                                );
                    }
                    if
                        (
                            onStatementCompletedEventHandlerProcessAction != null
                            &&
                            command is SqlCommand sqlCommand
                        )
                    {
                        sqlCommand
                            .StatementCompleted -=
                                onStatementCompletedEventHandlerProcessAction;
                        onStatementCompletedEventHandlerProcessAction = null;
                    }
                    if
                        (
                            onSqlInfoMessageEventHandlerProcessAction != null
                            &&
                            sqlConnection != null
                        )
                    {
                        sqlConnection
                            .InfoMessage -=
                                onSqlInfoMessageEventHandlerProcessAction;
                        onSqlInfoMessageEventHandlerProcessAction = null;
                    }
                }
            }
        }

        private
            (
                TDbCommand Command
                , List<TDbParameter> DbParameters
                , bool StatisticsEnabled
                , StatementCompletedEventHandler
                        OnStatementCompletedEventHandlerProcessAction
                , SqlInfoMessageEventHandler
                        OnSqlInfoMessageEventHandlerProcessAction
                , JObject Result
            ) 
            InitializeProcess
                        (
                            TDbConnection connection
                            , string storeProcedureName
                            , JToken inputsParameters
                            , int commandTimeoutInSeconds
                            , ExtensionInfo extensionInfo
                        )
        {
            bool statisticsEnabled = false;
            StatementCompletedEventHandler
                onStatementCompletedEventHandlerProcessAction = null;
            SqlInfoMessageEventHandler
                onSqlInfoMessageEventHandlerProcessAction = null;
            var command = new TDbCommand()
            {
                CommandType = CommandType.StoredProcedure
                , CommandTimeout = commandTimeoutInSeconds
                , CommandText = storeProcedureName
                , Connection = connection
            };
            var dbParameters = GenerateExecuteParameters
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
            var sqlConnection = connection as SqlConnection;
            if (connection != null)
            {
                statisticsEnabled = sqlConnection.StatisticsEnabled;
            }
            if (statisticsEnabled)
            {
                if (extensionInfo.messages == null)
                {
                    extensionInfo.messages = new JArray();
                }
                if (extensionInfo.recordCounts == null)
                {
                    extensionInfo.recordCounts = new JArray();
                }
                if (sqlConnection != null)
                {
                    onSqlInfoMessageEventHandlerProcessAction =
                    (sender, sqlInfoMessageEventArgs) =>
                    {
                        extensionInfo
                                .messageID++;
                        extensionInfo
                                .messages
                                .Add
                                    (
                                        new JObject()
                                        {
                                            {
                                                "MessageID"
                                                , extensionInfo
                                                        .messageID
                                            }
                                            ,
                                            {
                                                "ResultSetID"
                                                , extensionInfo
                                                        .resultSetID
                                            }
                                            ,
                                            {
                                                "Source"
                                                , sqlInfoMessageEventArgs
                                                                    .Source
                                            }
                                            ,
                                            {
                                                "Message"
                                                , sqlInfoMessageEventArgs
                                                                    .Message
                                            }
                                            ,
                                            {
                                                "DealTime"
                                                , DateTime.Now
                                            }
                                        }
                                    );
                    };
                    sqlConnection
                            .InfoMessage +=
                                onSqlInfoMessageEventHandlerProcessAction;
                }
                if (statisticsEnabled)
                {
                    if (command is SqlCommand sqlCommand)
                    {
                        onStatementCompletedEventHandlerProcessAction =
                            (sender, statementCompletedEventArgs) =>
                            {
                                extensionInfo
                                    .recordCounts
                                    .Add
                                        (
                                            statementCompletedEventArgs
                                                                .RecordCount
                                        );
                            };
                        sqlCommand
                            .StatementCompleted +=
                                onStatementCompletedEventHandlerProcessAction;

                    }
                }
            }
            return
                (
                    command
                    , dbParameters
                    , statisticsEnabled
                    , onStatementCompletedEventHandlerProcessAction
                    , onSqlInfoMessageEventHandlerProcessAction
                    , result
                );
        }
        public JToken
            Execute
                (
                    TDbConnection connection
                    , string storeProcedureName
                    , JToken inputsParameters = null //string.Empty
                    , Func
                        <
                            IDataReader
                            , Type          // fieldType
                            , string        // fieldName
                            , int           // row index
                            , int           // column index
                            ,
                                (
                                    bool NeedDefaultProcess
                                    , JProperty Field   //  JObject Field 对象
                                )
                        > onReadRowColumnProcessFunc = null
                    , int commandTimeoutInSeconds = 90
                )
        {
            var extensionInfo = new ExtensionInfo()
            {
                resultSetID = 0
                , messageID = 0
                , recordCounts = null
                , messages = null
            };
            (
                TDbCommand Command
                , List<TDbParameter> DbParameters
                , bool StatisticsEnabled
                , StatementCompletedEventHandler
                        OnStatementCompletedEventHandlerProcessAction
                , SqlInfoMessageEventHandler
                        OnSqlInfoMessageEventHandlerProcessAction
                , JObject Result
            )
            context = default;
            try
            {
                context = InitializeProcess
                (
                    connection
                    , storeProcedureName
                    , inputsParameters
                    , commandTimeoutInSeconds
                    , extensionInfo
                );
                connection.Open();
                var dataReader = context
                                    .Command
                                    .ExecuteReader
                                        (
                                            CommandBehavior
                                                .CloseConnection
                                        );
                do
                {
                    DataReadingProcess
                            (
                                onReadRowColumnProcessFunc
                                , context.Result
                                , dataReader
                            );
                    extensionInfo
                            .resultSetID++;
                }
                while (dataReader.NextResult());
                dataReader.Close();
                ResultProcess
                        (
                            connection
                            , context.StatisticsEnabled
                            , context.Command
                            , ref context.OnStatementCompletedEventHandlerProcessAction
                            , ref context.OnSqlInfoMessageEventHandlerProcessAction
                            , context.DbParameters
                            , context.Result
                            , extensionInfo
                        );
                return context.Result;
            }
            finally
            {
                extensionInfo.Clear();
                if (context.Command is SqlCommand sqlCommand)
                {
                    if (context.OnStatementCompletedEventHandlerProcessAction != null)
                    {
                        sqlCommand
                            .StatementCompleted -=
                                context.OnStatementCompletedEventHandlerProcessAction;
                    }
                    var sqlConnection = sqlCommand.Connection;
                    if (context.OnSqlInfoMessageEventHandlerProcessAction != null)
                    {
                        sqlConnection
                            .InfoMessage -=
                                context.OnSqlInfoMessageEventHandlerProcessAction;
                    }
                    if (sqlConnection.StatisticsEnabled)
                    {
                        sqlConnection.StatisticsEnabled = false;
                    }
                }
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
                context.Command.Dispose();
            }
        }

        public async Task<JToken>
            ExecuteAsync
                (
                    TDbConnection connection
                    , string storeProcedureName
                    , JToken inputsParameters = null //string.Empty
                    , Func
                        <
                            IDataReader
                            , Type          // fieldType
                            , string        // fieldName
                            , int           // row index
                            , int           // column index
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
            var extensionInfo = new ExtensionInfo()
            {
                resultSetID = 0
                , messageID = 0
                , recordCounts = null
                , messages = null
            };
            (
                TDbCommand Command
                , List<TDbParameter> DbParameters
                , bool StatisticsEnabled
                , StatementCompletedEventHandler
                        OnStatementCompletedEventHandlerProcessAction
                , SqlInfoMessageEventHandler
                        OnSqlInfoMessageEventHandlerProcessAction
                , JObject Result
            )
            context = default;
            try
            {
                context = InitializeProcess
                (
                    connection
                    , storeProcedureName
                    , inputsParameters
                    , commandTimeoutInSeconds
                    , extensionInfo
                );
                await
                    connection.OpenAsync();
                var dataReader =
                        await
                            context
                                .Command
                                .ExecuteReaderAsync
                                    (
                                        CommandBehavior
                                            .CloseConnection
                                    );
                do
                {
                    DataReadingProcess
                        (
                            onReadRowColumnProcessFunc
                            , context.Result
                            , dataReader
                        );
                    extensionInfo
                            .resultSetID++;
                }
                while
                    (
                        await
                            dataReader
                                .NextResultAsync()
                    );
                dataReader.Close();
                ResultProcess
                        (
                            connection
                            , context.StatisticsEnabled
                            , context.Command
                            , ref context.OnStatementCompletedEventHandlerProcessAction
                            , ref context.OnSqlInfoMessageEventHandlerProcessAction
                            , context.DbParameters
                            , context.Result
                            , extensionInfo
                        );
                return context.Result;
            }
            finally
            {
                extensionInfo.Clear();
                if (context.Command is SqlCommand sqlCommand)
                {
                    if (context.OnStatementCompletedEventHandlerProcessAction != null)
                    {
                        sqlCommand
                            .StatementCompleted -=
                                context.OnStatementCompletedEventHandlerProcessAction;
                    }
                    var sqlConnection = sqlCommand.Connection;
                    if (context.OnSqlInfoMessageEventHandlerProcessAction != null)
                    {
                        sqlConnection
                            .InfoMessage -=
                                context.OnSqlInfoMessageEventHandlerProcessAction;
                    }
                    if (sqlConnection.StatisticsEnabled)
                    {
                        sqlConnection.StatisticsEnabled = false;
                    }
                }
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
                context.Command.Dispose();
            }
        }
    }
}
