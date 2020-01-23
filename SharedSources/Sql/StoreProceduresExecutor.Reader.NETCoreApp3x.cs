#if NETCOREAPP3_X || NETSTANDARD2_X
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

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
        public virtual async
                IAsyncEnumerable
                    <
                        (
                            int             // resultSetIndex
                            , int           // rowIndex
                            , JArray        // columns
                            , IDataRecord
                        )
                    >

            ExecuteResultsAsAsyncEnumerable
            (
                    TDbConnection connection
                    , string storeProcedureName
                    , JToken inputsParameters = null //string.Empty
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
                await 
                    connection
                            .OpenAsync();
                var dataReader = command
                                    .ExecuteReader
                                        (
                                            CommandBehavior
                                                .CloseConnection
                                        );
                var entries = dataReader
                                    .AsMultipleResultsIAsyncEnumerable();
                                        //(
                                        //    (resultSetIndex, rowIndex, columns, dataRecord) =>
                                        //    {
                                        //        return dataRecord;
                                        //    }
                                        //);
                await
                    foreach //(var entry in entries)
                            (
                                var 
                                    (
                                        resultSetIndex
                                        , rowIndex
                                        , columns
                                        , dataRecord
                                    )
                                in
                                entries
                            )
                {
                    extensionInfo
                            .resultSetID = resultSetIndex;
                    yield
                        return
                            (
                                resultSetIndex
                                , rowIndex
                                , columns
                                , dataRecord
                            );
                }
                await 
                    dataReader
                            .CloseAsync();
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
                if (_needAutoRefreshExecutedTimeForSlideExpire)
                {
                    RefreshCachedExecuted
                                    (
                                        connection
                                        , storeProcedureName
                                    );
                }
                if (connection.State != ConnectionState.Closed)
                {
                    await
                        connection
                                .CloseAsync();
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
#endif