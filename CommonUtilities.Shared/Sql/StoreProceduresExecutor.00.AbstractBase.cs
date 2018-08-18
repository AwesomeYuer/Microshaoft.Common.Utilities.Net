﻿namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    public abstract class
                    AbstractStoreProceduresExecutor
                            <TDbConnection, TDbCommand, TDbParameter>
                                where
                                        TDbConnection : DbConnection, new()
                                where
                                        TDbCommand : DbCommand, new()
                                where
                                        TDbParameter : DbParameter, new()
    {
        public int CachedExecutingParametersExpiredInSeconds
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
                    var cloneDbParameter = dbParameter
                                                .ShallowClone
                                                    <TDbParameter>
                                                        (
                                                            OnExecutingSetDbParameterTypeProcess
                                                        );
                    var parameterValue =
                            OnExecutingSetDbParameterValueProcess
                                    (
                                        cloneDbParameter
                                        , jProperty.Value
                                    );
                    var dbParameterValue = dbParameter.Value;
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
                            dataTable = dataTable.Clone();
                            var jArray = (JArray) jProperty.Value;
                            var columns = dataTable.Columns;
                            var rows = dataTable.Rows;
                            foreach (var entry in jArray)
                            {
                                var row = dataTable.NewRow();
                                foreach (DataColumn column in columns)
                                {
                                    var columnName = column.ColumnName;
                                    row[columnName] = entry[columnName];
                                }
                                rows.Add(row);
                            }
                        }
                        cloneDbParameter.Value = dataTable;
                    }
                    else
                    {
                        cloneDbParameter.Value = parameterValue;
                    }
                    if (result == null)
                    {
                        result = new List<TDbParameter>();
                    }
                    result.Add(cloneDbParameter);
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
                        var cloneDbParameter = dbParameter
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
            public DateTime RecentExecutedTime;
        }
        private
            ConcurrentDictionary<string, ExecutingInfo>
                _dictionary
                    = new ConcurrentDictionary<string, ExecutingInfo>
                            (
                                StringComparer.OrdinalIgnoreCase
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
                    .RecentExecutedTime = DateTime.Now;
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
                var dbParameters = GetDefinitionParameters
                                (
                                    connectionString
                                    , storeProcedureName
                                    , includeReturnValueParameter
                                );
                var parameters =
                        dbParameters
                            .ToDictionary
                                (
                                    (xx) =>
                                    {
                                        return
                                            xx
                                                .ParameterName
                                                .TrimStart('@');
                                    }
                                    , (xx) =>
                                    {
                                        return
                                            (DbParameter)xx;
                                    }
                                    , StringComparer
                                            .OrdinalIgnoreCase
                                );
                var _executingInfo = new ExecutingInfo()
                {
                    DbParameters = parameters,
                    RecentExecutedTime = DateTime.Now
                };
                return _executingInfo;
            }

            DbConnection connection = new TDbConnection();
            connection.ConnectionString = connectionString;
            var key = $"{connection.DataSource}-{connection.Database}-{storeProcedureName}".ToUpper();
            var add = false;
            var executingInfo
                    = _dictionary
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
                if (CachedExecutingParametersExpiredInSeconds > 0)
                {
                    if
                        (
                            DateTimeHelper
                                .SecondsDiffNow
                                    (
                                        executingInfo
                                                .RecentExecutedTime
                                    )
                            > CachedExecutingParametersExpiredInSeconds
                        )
                    {
                        executingInfo = GetExecutingInfo();
                        _dictionary[key] = executingInfo;
                        result = executingInfo.DbParameters;
                    }
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
        public ParameterDirection GetParameterDirection(string parameterMode)
        {
            var r = SqlHelper.GetParameterDirection(parameterMode);
            return r;
        }
        /// <summary>
        /// Converts the OleDb parameter direction
        /// </summary>
        /// <param name="oledbDirection">The integer parameter direction</param>
        /// <returns>A ParameterDirection</returns>
        //public ParameterDirection GetParameterDirection(short oledbDirection)
        //{
        //    var r = SqlHelper.GetParameterDirection(oledbDirection);
        //    return r;
        //}

        public JToken
                    Execute

                                    (
                                        DbConnection connection
                                        , string storeProcedureName
                                        , string p = null //string.Empty
                                        , int commandTimeout = 90
                                    )
         {
            var inputsParameters = JObject.Parse(p);
            return
                Execute

                        (
                            connection
                            , storeProcedureName
                            , inputsParameters
                            , commandTimeout
                        );
        }

        public JToken
                    Execute
                        (
                            DbConnection connection
                            , string storeProcedureName
                            , JToken inputsParameters = null //string.Empty
                            , int commandTimeout = 90
                        )
        {
            var dataSource = connection.DataSource;
            var dataBaseName = connection.Database;
            try
            {
                using
                    (
                        TDbCommand command = new TDbCommand()
                        {
                            CommandType = CommandType.StoredProcedure
                            ,
                            CommandTimeout = commandTimeout
                            ,
                            CommandText = storeProcedureName
                            ,
                            Connection = connection
                        }
                    )
                {
                    List<TDbParameter>
                        dbParameters
                            = GenerateExecuteParameters
                                    //<TDbConnection, TDbCommand, TDbParameter>
                                        (
                                            connection.ConnectionString
                                            , storeProcedureName
                                            , inputsParameters
                                            //, onQueryDefinitionsSetInputParameterProcessFunc
                                            //, onQueryDefinitionsSetReturnParameterProcessFunc
                                            //, onQueryDefinitionsReadOneDbParameterProcessFunc
                                            //, onExecutingSetDbParameterTypeProcessFunc
                                            //, onExecutingSetDbParameterValueProcessFunc
                                        );
                    if (dbParameters != null)
                    {
                        var parameters = dbParameters.ToArray();
                        command.Parameters.AddRange(parameters);
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
                            "Inputs"
                            , new JObject
                                {
                                    {
                                        "Parameters"
                                            , inputsParameters
                                    }
                                }
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
                    do
                    {
                        var columns = dataReader
                                        .GetJTokensColumnsEnumerable();
                        var rows = dataReader
                                        .AsJTokensRowsEnumerable();

                        (
                            (JArray)
                                result
                                    ["Outputs"]["ResultSets"]
                        )
                            .Add
                                (
                                    new JObject
                                    {
                                        {
                                            "Columns"
                                            , new JArray(columns)
                                        }
                                        ,
                                        {
                                            "Rows"
                                            , new JArray(rows)
                                        }
                                    }
                                );
                    }
                    while (dataReader.NextResult());
                    dataReader.Close();
                    JObject jOutputParameters = null;
                    if (dbParameters != null)
                    {
                        var outputParameters
                                = dbParameters
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
                                        x.ParameterName.TrimStart('@')
                                        , new JValue(x.Value)
                                    );
                        }
                    }
                    if (jOutputParameters != null)
                    {
                        result["Outputs"]["Parameters"] = jOutputParameters;
                    }
                    return result;
                }
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
                connection = null;
            }
        }
    }
}