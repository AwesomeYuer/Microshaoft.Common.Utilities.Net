namespace Microshaoft.CompositionPlugins
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;

    public abstract partial class
            AbstractStoreProcedureExecutorCompositionPlugin
                        <TDbConnection, TDbCommand, TDbParameter>
                            : IStoreProcedureExecutable
                                , IParametersDefinitionCacheAutoRefreshable
                            where
                                    TDbConnection : DbConnection, new()
                            where
                                    TDbCommand : DbCommand, new()
                            where
                                    TDbParameter : DbParameter, new()
    {
        public abstract
                    AbstractStoreProceduresExecutor
                        <TDbConnection, TDbCommand, TDbParameter>
                            Executor
        {
            set;
            get;
        }
        public abstract string DataBaseType
        {
            get;
        }
        public virtual int CachedParametersDefinitionExpiredInSeconds
        {
            get;
            set;
        }
        public virtual bool NeedAutoRefreshExecutedTimeForSlideExpire
        {
            get;
            set;
        }
        protected virtual void AfterExecutedProcess
                                    (
                                        string storeProcedureName
                                        , TDbConnection connection
                                    )
        {
            if (NeedAutoRefreshExecutedTimeForSlideExpire)
            {
                Executor
                    .RefreshCachedExecuted
                        (
                            connection
                            , storeProcedureName
                        );
            }
        }
        protected virtual void BeforeExecutingProcess
                        (
                            string connectionString
                            , bool enableStatistics
                            , out TDbConnection connection
                        )
        {
            if
                (
                    CachedParametersDefinitionExpiredInSeconds > 0
                    &&
                    Executor
                        .CachedParametersDefinitionExpiredInSeconds
                    !=
                    CachedParametersDefinitionExpiredInSeconds
                )
            {
                Executor
                        .CachedParametersDefinitionExpiredInSeconds
                            = CachedParametersDefinitionExpiredInSeconds;
            }
            connection = new TDbConnection
            {
                ConnectionString = connectionString
            };
        }
        public virtual 
            (
                bool Success
                , JToken Result
            )
                Execute
                    (
                        string connectionString
                        , string storeProcedureName
                        , JToken parameters
                        , Func
                                <
                                    int             // resultSet index
                                    , IDataReader
                                    , int           // row index
                                    , int           // column index
                                    , Type          // fieldType
                                    , string        // fieldName
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
            BeforeExecutingProcess
                    (
                        connectionString
                        , enableStatistics
                        , out TDbConnection connection
                    );
            var result = Executor
                                .ExecuteJsonResults
                                    (
                                        connection
                                        , storeProcedureName
                                        , parameters
                                        , onReadRowColumnProcessFunc
                                        , commandTimeoutInSeconds
                                    );
            AfterExecutedProcess
                (
                    storeProcedureName
                    , connection
                );
            return
                (
                    Success: result != null
                    , Result: result
                );
        }
        public virtual async
            Task
                <
                    (
                        bool Success
                        , JToken Result
                    )
                > 
                    ExecuteAsync
                            (
                                string connectionString
                                , string storeProcedureName
                                , JToken parameters
                                , Func
                                        <
                                            int             // resultSet index
                                            , IDataReader
                                            , int           // row index
                                            , int           // column index
                                            , Type          // fieldType
                                            , string        // fieldName
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
            BeforeExecutingProcess
                    (
                        connectionString
                        , enableStatistics
                        , out TDbConnection connection
                    );
            var result = await
                            Executor
                                    .ExecuteJsonResultsAsync
                                            (
                                                connection
                                                , storeProcedureName
                                                , parameters
                                                , onReadRowColumnProcessFunc
                                                , commandTimeoutInSeconds
                                            );
            AfterExecutedProcess
                (
                    storeProcedureName
                    , connection
                );
            return
                (
                    Success: result != null
                    , Result: result
                );
        }

        public void ExecuteReaderRows
                            (
                                string connectionString
                                , string storeProcedureName
                                , JToken parameters = null
                                , Action
                                        <
                                            int
                                            , JArray
                                            , IDataReader
                                            , int
                                        > onReadRowProcessAction = null
                                , bool enableStatistics = false
                                , int commandTimeoutInSeconds = 90
                            )
        {
            BeforeExecutingProcess
                    (
                        connectionString
                        , enableStatistics
                        , out TDbConnection connection
                    );
            Executor
                    .ExecuteReaderProcess
                        (
                            connection
                            , storeProcedureName
                            , parameters
                            , (resultSetIndex, reader, columns, rowIndex) =>
                            {
                                onReadRowProcessAction(resultSetIndex, reader, columns, rowIndex);
                            }
                            , commandTimeoutInSeconds
                        );
            AfterExecutedProcess
                (
                    storeProcedureName
                    , connection
                );
        }

        public async Task ExecuteReaderRowsAsync
                                (
                                    string connectionString
                                    , string storeProcedureName
                                    , JToken parameters = null
                                    , Func
                                        <
                                            int
                                            , JArray
                                            , IDataReader
                                            , int
                                            , Task
                                        >
                                            onReadRowProcessActionAsync = null
                                    , bool enableStatistics = false
                                    , int commandTimeoutInSeconds = 90
                                )
        {
            BeforeExecutingProcess
                    (
                        connectionString
                        , enableStatistics
                        , out TDbConnection connection
                    );
            await
                Executor
                    .ExecuteReaderProcessAsync
                        (
                            connection
                            , storeProcedureName
                            , parameters
                            , async (resultSetIndex, columns, reader, rowIndex) =>
                            {
                                await
                                    onReadRowProcessActionAsync
                                        (
                                            resultSetIndex
                                            , reader
                                            , columns
                                            , rowIndex
                                        );
                            }
                            , commandTimeoutInSeconds
                        );
            AfterExecutedProcess
                (
                    storeProcedureName
                    , connection
                );
        }
    }
}
