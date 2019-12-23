namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
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
        public void ExecuteReaderRows
            (
                    TDbConnection connection
                    , string storeProcedureName
                    , JToken inputsParameters = null //string.Empty
                    , Action
                            <
                                int             // resultSetID
                                , JArray
                                , IDataReader
                                , int           // row index
                            > onReadRowProcessAction = null
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

            TDbCommand command = null;
            List<TDbParameter> dbParameters;
            bool statisticsEnabled;
            StatementCompletedEventHandler
                    onStatementCompletedEventHandlerProcessAction = null;
            SqlInfoMessageEventHandler
                    onSqlInfoMessageEventHandlerProcessAction = null;
            JObject result;
            try
            {
                (
                    command
                    , dbParameters
                    , statisticsEnabled
                    , onStatementCompletedEventHandlerProcessAction
                    , onSqlInfoMessageEventHandlerProcessAction
                    , result
                ) = ResultPreprocess
                        (
                            connection
                            , storeProcedureName
                            , inputsParameters
                            , commandTimeoutInSeconds
                            , extensionInfo
                        );
                connection
                        .Open();
                var dataReader = command
                                    .ExecuteReader
                                        (
                                            CommandBehavior
                                                .CloseConnection
                                        );
                do
                {
                    JArray jColumns = null;
                    if (onReadRowProcessAction != null)
                    {
                        if (jColumns == null)
                        {
                            jColumns = dataReader
                                            .GetColumnsJArray();
                        }
                        dataReader
                                .ReadRows
                                    (
                                        jColumns
                                        , (reader, columns, rowIndex) =>
                                        {
                                            onReadRowProcessAction
                                                (
                                                    extensionInfo.resultSetID
                                                    , columns
                                                    , reader
                                                    , rowIndex
                                                );
                                        }
                                    );
                    }
                    extensionInfo
                            .resultSetID ++;
                    jColumns = null;
                }
                while
                    (
                        dataReader
                            .NextResult()
                    );
                dataReader.Close();
            }
            finally
            {
                extensionInfo.Clear();
                if (onStatementCompletedEventHandlerProcessAction != null)
                {
                    if (command is SqlCommand sqlCommand)
                    {
                        sqlCommand
                            .StatementCompleted -=
                                onStatementCompletedEventHandlerProcessAction;
                    }
                }
                if (onSqlInfoMessageEventHandlerProcessAction != null)
                {
                    if (connection is SqlConnection sqlConnection)
                    {
                        sqlConnection
                            .InfoMessage -=
                                onSqlInfoMessageEventHandlerProcessAction;
                        if (sqlConnection.StatisticsEnabled)
                        {
                            sqlConnection.StatisticsEnabled = false;
                        }
                    }
                }
                if (connection.State != ConnectionState.Closed)
                {
                    connection
                            .Close();
                }
                if (command != null)
                {
                    command
                        .Dispose();
                }
            }
        }

        public async Task ExecuteReaderRowsAsync
                (
                        TDbConnection connection
                        , string storeProcedureName
                        , JToken inputsParameters = null //string.Empty
                        , Func
                                <
                                    int             // resultSetID
                                    , IDataReader
                                    , JArray
                                    , int           // row index
                                    , Task
                                > onReadRowProcessActionAsync = null
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

            TDbCommand command = null;
            List<TDbParameter> dbParameters;
            bool statisticsEnabled;
            StatementCompletedEventHandler
                    onStatementCompletedEventHandlerProcessAction = null;
            SqlInfoMessageEventHandler
                    onSqlInfoMessageEventHandlerProcessAction = null;
            JObject result;
            try
            {
                (
                    command
                    , dbParameters
                    , statisticsEnabled
                    , onStatementCompletedEventHandlerProcessAction
                    , onSqlInfoMessageEventHandlerProcessAction
                    , result
                ) = ResultPreprocess
                        (
                            connection
                            , storeProcedureName
                            , inputsParameters
                            , commandTimeoutInSeconds
                            , extensionInfo
                        );
                connection
                        .Open();
                var dataReader = await 
                                    command
                                        .ExecuteReaderAsync
                                            (
                                                CommandBehavior
                                                    .CloseConnection
                                            );
                do
                {
                    JArray jColumns = null;
                    if (onReadRowProcessActionAsync != null)
                    {
                        if (jColumns == null)
                        {
                            jColumns = dataReader
                                            .GetColumnsJArray();
                        }
                        await
                            dataReader
                                    .ReadRowsAsync
                                        (
                                            jColumns
                                            , async (reader, columns, rowIndex) =>
                                            {
                                                await
                                                    onReadRowProcessActionAsync
                                                        (
                                                            extensionInfo
                                                                    .resultSetID
                                                            , reader
                                                            , columns
                                                            , rowIndex
                                                        );
                                            }
                                        );
                    }
                    extensionInfo
                            .resultSetID ++;
                    jColumns = null;
                }
                while
                    (
                        await
                            dataReader
                                .NextResultAsync()
                    );
#if NETCOREAPP3_X
                await
                    dataReader
                            .CloseAsync();
#else
                dataReader
                        .Close();
#endif
            }
            finally
            {
                extensionInfo.Clear();
                if (onStatementCompletedEventHandlerProcessAction != null)
                {
                    if (command is SqlCommand sqlCommand)
                    {
                        sqlCommand
                            .StatementCompleted -=
                                onStatementCompletedEventHandlerProcessAction;
                    }
                }
                if (onSqlInfoMessageEventHandlerProcessAction != null)
                {
                    if (connection is SqlConnection sqlConnection)
                    {
                        sqlConnection
                            .InfoMessage -=
                                onSqlInfoMessageEventHandlerProcessAction;
                        if (sqlConnection.StatisticsEnabled)
                        {
                            sqlConnection.StatisticsEnabled = false;
                        }
                    }
                }
                if (connection.State != ConnectionState.Closed)
                {
                    connection
                            .Close();
                }
                if (command != null)
                {
                    command
                        .Dispose();
                }
            }
        }

    }
}
