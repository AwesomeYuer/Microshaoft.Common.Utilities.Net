namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;

    public class ExecutingInfo
    {
        public IDictionary<string, DbParameter> DbParameters;
        public DateTime RecentParametersDefinitionCacheUsedTime;
        public object Locker = new object();
    }

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
        public ConcurrentDictionary<string, ExecutingInfo> Cache
        {
            get => _dictionary;
            private set => _dictionary = value;
        }

        private string _parametersQueryCommandText = $@"
                    SELECT
                        * 
                    FROM
                        information_schema.parameters a 
                    WHERE
                        a.SPECIFIC_NAME = @ProcedureName
                    ";

        public virtual string ParametersQueryCommandText
        {
            get => _parametersQueryCommandText;
            //set => _parametersQueryCommandText = value;
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
            var jProperties = (JObject) inputsParameters;
            foreach (KeyValuePair<string, JToken> jProperty in jProperties)
            {
                if
                    (
                        dbParameters
                                .TryGetValue
                                    (
                                        jProperty.Key
                                        , out DbParameter dbParameter
                                    )
                    )
                {
                    var direction = dbParameter
                                            .Direction;
                    var dbParameterValue = dbParameter
                                                    .Value;
                    var includeValueClone = false;
                    if
                        (
                            dbParameterValue != DBNull.Value
                            &&
                            dbParameterValue != null
                        )
                    {
                        if (dbParameterValue is DataTable)
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
            if
                (
                    _dictionary
                        .TryGetValue
                            (
                                $"{connection.DataSource}-{connection.Database}-{storeProcedureName}"
                                , out ExecutingInfo executingInfo
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
            var connection = new TDbConnection
            {
                ConnectionString = connectionString
            };
            var add = false;
            var executingInfo = _dictionary
                                        .GetOrAdd
                                                (
                                                    $"{connection.DataSource}-{connection.Database}-{storeProcedureName}"
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
        public virtual IEnumerable<TDbParameter>
                                GetDefinitionParameters
                                        (
                                            string connectionString
                                            , string storeProcedureName
                                            , bool includeReturnValueParameter = false
                                        )
        {
            return
                SqlHelper
                    .GetStoreProcedureDefinitionParameters
                            <TDbConnection, TDbCommand, TDbParameter>
                                (
                                    connectionString
                                    , storeProcedureName
                                    , OnQueryDefinitionsSetInputParameterProcess
                                    , OnQueryDefinitionsSetReturnParameterProcess
                                    , OnQueryDefinitionsReadOneDbParameterProcess
                                    , ParametersQueryCommandText
                                    , includeReturnValueParameter
                                );
        }
        public IDictionary<string, DbParameter>
                        GetNameIndexedDefinitionParameters
                                (
                                    string connectionString
                                    , string storeProcedureName
                                    , bool includeReturnValueParameter = false
                                )

        {
            var dbParameters = GetDefinitionParameters
                                    (
                                        connectionString
                                        , storeProcedureName
                                        , includeReturnValueParameter
                                    );
            return
                dbParameters
                        .ToDictionary
                            (
                                (x) =>
                                {
                                    return
                                            x
                                                .ParameterName
                                                .TrimStart('@', '?');
                                }
                                , (x) =>
                                {
                                    return
                                        (DbParameter) x;
                                }
                                , StringComparer
                                        .OrdinalIgnoreCase
                            );
        }
        public ParameterDirection GetParameterDirection(string parameterMode)
        {
            return
                SqlHelper
                    .GetParameterDirection
                        (parameterMode);
        }

        protected IEnumerable<TDbParameter> GetStoreProcedureDefinitionParameters
            (
                string connectionString
                , string storeProcedureName
                , string parametersQueryCommandText = null
                , bool includeReturnValueParameter = false
            )
        {
            if (parametersQueryCommandText.IsNullOrEmptyOrWhiteSpace())
            {
                parametersQueryCommandText = _parametersQueryCommandText;
            }
            return
                SqlHelper
                    .GetStoreProcedureDefinitionParameters
                            <TDbConnection, TDbCommand, TDbParameter>
                                (
                                    connectionString
                                    , storeProcedureName
                                    , OnQueryDefinitionsSetInputParameterProcess
                                    , OnQueryDefinitionsSetReturnParameterProcess
                                    , OnQueryDefinitionsReadOneDbParameterProcess
                                    , ParametersQueryCommandText
                                    , includeReturnValueParameter
                                );
        }
    }
}
