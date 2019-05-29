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

        private class AdditionalInfo
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
                                SqlConnection sqlConnection
                                , bool statisticsEnabled
                                , SqlCommand sqlCommand
                                , ref StatementCompletedEventHandler
                                            onStatementCompletedEventHandlerProcessAction
                                , ref SqlInfoMessageEventHandler
                                            onSqlInfoMessageEventHandlerProcessAction
                                , List<TDbParameter> dbParameters
                                , JObject result
                                , AdditionalInfo additionalInfo
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
                    if (additionalInfo.messages != null)
                    {
                        result
                            ["DataBaseStatistics"]
                            ["Messages"] = additionalInfo.messages;
                    }
                    if (additionalInfo.recordCounts != null)
                    {
                        jCurrent
                            .Parent
                            .AddAfterSelf
                                (
                                    new JProperty
                                            (
                                                "RecordCounts"
                                                , additionalInfo
                                                        .recordCounts
                                            )
                                );
                    }
                }
                if (onStatementCompletedEventHandlerProcessAction != null)
                {
                    sqlCommand
                        .StatementCompleted -=
                            onStatementCompletedEventHandlerProcessAction;
                    onStatementCompletedEventHandlerProcessAction = null;
                }
                if (onSqlInfoMessageEventHandlerProcessAction != null)
                {
                    sqlConnection
                        .InfoMessage -=
                            onSqlInfoMessageEventHandlerProcessAction;
                    onSqlInfoMessageEventHandlerProcessAction = null;
                }
            }
        }

        private void InitializeProcess
                        (
                            DbConnection connection
                            , string storeProcedureName
                            , JToken inputsParameters
                            , int commandTimeoutInSeconds
                            , out SqlConnection sqlConnection
                            , out bool isSqlConnection
                            , out bool statisticsEnabled
                            , out SqlCommand sqlCommand
                            , out StatementCompletedEventHandler
                                        onStatementCompletedEventHandlerProcessAction
                            , out SqlInfoMessageEventHandler
                                        onSqlInfoMessageEventHandlerProcessAction
                            , out TDbCommand command
                            , out List<TDbParameter> dbParameters
                            , out JObject result
                            , AdditionalInfo additionalInfo
                        )
        {
            var dataSource = connection.DataSource;
            var dataBaseName = connection.Database;
            sqlConnection = connection as SqlConnection;
            isSqlConnection = (sqlConnection != null);
            statisticsEnabled = false;
            if (isSqlConnection)
            {
                statisticsEnabled = sqlConnection.StatisticsEnabled;
            }
            sqlCommand = null;
            onStatementCompletedEventHandlerProcessAction = null;
            onSqlInfoMessageEventHandlerProcessAction = null;
            //try
            //{
            //using
            //    (
            command = new TDbCommand()
            {
                CommandType = CommandType.StoredProcedure
                , CommandTimeout = commandTimeoutInSeconds
                , CommandText = storeProcedureName
                , Connection = connection
            };
            //)
            //{

            if (commandTimeoutInSeconds > 0)
            {
                command
                    .CommandTimeout = commandTimeoutInSeconds;
            }
            dbParameters = GenerateExecuteParameters
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
            result = new JObject
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
            if (statisticsEnabled)
            {
                if (additionalInfo.messages == null)
                {
                    additionalInfo.messages = new JArray();
                }
                if (additionalInfo.recordCounts == null)
                {
                    additionalInfo.recordCounts = new JArray();
                }
                sqlCommand = command as SqlCommand;
                onStatementCompletedEventHandlerProcessAction =
                    (sender, statementCompletedEventArgs) =>
                        {
                            additionalInfo
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
                onSqlInfoMessageEventHandlerProcessAction =
                (sender, sqlInfoMessageEventArgs) =>
                        {
                            additionalInfo
                                .messageID ++;
                            additionalInfo
                                    .messages
                                    .Add
                                        (
                                            new JObject()
                                            {
                                                {
                                                    "MessageID"
                                                    , additionalInfo
                                                            .messageID
                                                }
                                                ,
                                                {
                                                    "ResultSetID"
                                                    , additionalInfo
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
            SqlConnection sqlConnection = null;
            bool isSqlConnection = false;
            bool statisticsEnabled;
            SqlCommand sqlCommand = null;
            StatementCompletedEventHandler
                    onStatementCompletedEventHandlerProcessAction = null;
            SqlInfoMessageEventHandler
                    onSqlInfoMessageEventHandlerProcessAction = null;
            TDbCommand command = null;
            List<TDbParameter> dbParameters = null;
            JObject result = null;

            var additionalInfo = new AdditionalInfo()
            {
                resultSetID = 0
                , messageID = 0
                , recordCounts = null
                , messages = null
            };

            try
            {
                InitializeProcess
                    (
                        connection
                        , storeProcedureName
                        , inputsParameters
                        , commandTimeoutInSeconds
                        , out sqlConnection
                        , out isSqlConnection
                        , out statisticsEnabled
                        , out sqlCommand
                        , out onStatementCompletedEventHandlerProcessAction
                        , out onSqlInfoMessageEventHandlerProcessAction
                        , out command
                        , out dbParameters
                        , out result
                        , additionalInfo
                    );
                connection.Open();
                var dataReader = command
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
                                , result
                                , dataReader
                            );
                    additionalInfo.resultSetID++;
                }
                while (dataReader.NextResult());
                dataReader.Close();
                ResultProcess
                        (
                            sqlConnection
                            , statisticsEnabled
                            , sqlCommand
                            , ref onStatementCompletedEventHandlerProcessAction
                            , ref onSqlInfoMessageEventHandlerProcessAction
                            , dbParameters
                            , result
                            , additionalInfo
                        );
                return result;
            }
            finally
            {
                additionalInfo.Clear();
                //additionalInfo = null;
                if (isSqlConnection)
                {
                    if (onStatementCompletedEventHandlerProcessAction != null)
                    {
                        sqlCommand
                            .StatementCompleted -=
                                onStatementCompletedEventHandlerProcessAction;
                        //onStatementCompletedEventHandlerProcessAction = null;
                        //sqlCommand = null;
                    }
                    if (onSqlInfoMessageEventHandlerProcessAction != null)
                    {
                        sqlConnection
                            .InfoMessage -=
                                onSqlInfoMessageEventHandlerProcessAction;
                        //onSqlInfoMessageEventHandlerProcessAction = null;
                    }
                    if (sqlConnection.StatisticsEnabled)
                    {
                        sqlConnection.StatisticsEnabled = false;
                    }
                    //sqlConnection = null;
                }
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
                command.Dispose();
                //command = null;
                //dbParameters = null;
            }
        }

        public async Task<JToken>
            ExecuteAsync
                (
                    DbConnection connection
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
            SqlConnection sqlConnection = null;
            bool isSqlConnection = false;
            SqlCommand sqlCommand = null;
            StatementCompletedEventHandler
                    onStatementCompletedEventHandlerProcessAction = null;
            SqlInfoMessageEventHandler
                    onSqlInfoMessageEventHandlerProcessAction = null;
            TDbCommand command = null;
            List<TDbParameter> dbParameters = null;
            JObject result = null;

            var additionalInfo = new AdditionalInfo()
            {
                resultSetID = 0
                , messageID = 0
                , recordCounts = null
                , messages = null
            };

            try
            {
                InitializeProcess
                    (
                        connection
                        , storeProcedureName
                        , inputsParameters
                        , commandTimeoutInSeconds
                        , out sqlConnection
                        , out isSqlConnection
                        , out bool statisticsEnabled
                        , out sqlCommand
                        , out onStatementCompletedEventHandlerProcessAction
                        , out onSqlInfoMessageEventHandlerProcessAction
                        , out command
                        , out dbParameters
                        , out result
                        , additionalInfo
                    );
                connection.Open();
                var dataReader =
                        await
                            command
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
                            , result
                            , dataReader
                        );
                    additionalInfo
                            .resultSetID ++;
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
                        sqlConnection
                        , statisticsEnabled
                        , sqlCommand
                        , ref onStatementCompletedEventHandlerProcessAction
                        , ref onSqlInfoMessageEventHandlerProcessAction
                        , dbParameters
                        , result
                        , additionalInfo
                    );
                return result;
            }
            finally
            {
                additionalInfo.Clear();
                //additionalInfo = null;
                if (isSqlConnection)
                {
                    if (onStatementCompletedEventHandlerProcessAction != null)
                    {
                        sqlCommand
                            .StatementCompleted -=
                                onStatementCompletedEventHandlerProcessAction;
                        //onStatementCompletedEventHandlerProcessAction = null;
                        //sqlCommand = null;
                    }
                    if (onSqlInfoMessageEventHandlerProcessAction != null)
                    {
                        sqlConnection
                            .InfoMessage -=
                                onSqlInfoMessageEventHandlerProcessAction;
                        //onSqlInfoMessageEventHandlerProcessAction = null;
                    }
                    if (sqlConnection.StatisticsEnabled)
                    {
                        sqlConnection.StatisticsEnabled = false;
                    }
                    //sqlConnection = null;
                }
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
                command.Dispose();
                //command = null;
                //dbParameters = null;
            }
        }
    }
}