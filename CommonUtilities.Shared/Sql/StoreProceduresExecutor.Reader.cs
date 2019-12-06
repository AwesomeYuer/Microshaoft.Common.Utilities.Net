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
        public void ExecuteRows
            (
                    TDbConnection connection
                    , string storeProcedureName
                    , JToken inputsParameters = null //string.Empty
                    , Action
                            <
                                int             // resultSetID
                                , IDataReader
                                , JArray           
                                , int           // row index
                            > onReadRowProcessAction = null
                    , int commandTimeoutInSeconds = 90
            )
        {
            var extensionInfo = new ExtensionInfo()
            {
                resultSetID = 0
                    ,
                messageID = 0
                    ,
                recordCounts = null
                    ,
                messages = null
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
                    if (jColumns == null)
                    {
                        jColumns = dataReader.GetColumnsJArray();
                    }
                    if (onReadRowProcessAction != null)
                    {
                        dataReader
                                .ReadRows
                                    (
                                        jColumns
                                        , (reader, columns, rowIndex) =>
                                        {
                                            onReadRowProcessAction
                                                (
                                                    extensionInfo.resultSetID
                                                    , reader
                                                    , columns
                                                    , rowIndex
                                                );
                                        }
                                    );
                    }
                    extensionInfo
                            .resultSetID++;
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
    }
}
