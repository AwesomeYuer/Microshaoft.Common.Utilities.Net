#if NETCOREAPP3_X
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

            ExecuteReaderAsAsyncEnumerable
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
                await foreach (var entry in entries)
                {
                    extensionInfo
                            .resultSetID = entry.Item1;
                    yield
                        return
                            (
                                entry.Item1
                                , entry.Item2
                                , entry.Item3
                                , entry.Item4
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
#endif