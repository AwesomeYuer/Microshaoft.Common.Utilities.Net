namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    public static class SqlHelper
    {
        public static JValue 
                            GetJValue
                                <TDbParameter>
                                    (
                                        this DbParameter target
                                        , Func<TDbParameter, JValue>
                                                onDbParameterToJValueProcessFunc
                                    )
                            where
                                TDbParameter : DbParameter
        {
            JValue r = null;
            TDbParameter dbParameter = (TDbParameter) target;
            r = onDbParameterToJValueProcessFunc(dbParameter);
            return r;
        }
        public static TDbParameter ShallowClone<TDbParameter>
                        (
                            this DbParameter target
                            , Func<TDbParameter, TDbParameter, TDbParameter>
                                    onSetTypeProcessFunc
                            , bool includeValue = false
                        )
                            where
                                TDbParameter : DbParameter , new()
        {
            var clone = new TDbParameter();
            clone.ParameterName = target.ParameterName;
            clone.Size = target.Size;
            clone.Direction = target.Direction;
            clone.Scale = target.Scale;
            clone.Precision = target.Precision;
            if (includeValue)
            {
                //Shadow copy
                clone.Value = target.Value;
            }
            clone = onSetTypeProcessFunc
                            (
                                (TDbParameter) target
                                , clone
                            );
            return clone;
        }
        public static int CachedExecutingParametersExpiredInSeconds
        {
            get;
            set;
        }
        public static List<TDbParameter> 
                    GenerateStoreProcedureExecuteParameters
                        <TDbConnection, TDbCommand, TDbParameter>
                            (
                                string connectionString
                                , string storeProcedureName
                                , JToken inputsParameters
                                , Func<TDbParameter, TDbParameter>
                                        onQueryDefinitionsSetInputParameterProcessFunc
                                , Func<TDbParameter, TDbParameter>
                                        onQueryDefinitionsSetReturnParameterProcessFunc
                                , Func<IDataReader, TDbParameter, TDbParameter>
                                        onQueryDefinitionsReadOneDbParameterProcessFunc
                                , Func<TDbParameter, TDbParameter, TDbParameter>
                                        onExecutingSetDbParameterTypeProcessFunc
                                , Func<TDbParameter, JToken , object>
                                        onExecutingSetDbParameterValueProcessFunc
                            )
                        where
                                TDbConnection : DbConnection, new()
                        where
                                TDbCommand : DbCommand, new()
                        where
                                TDbParameter : DbParameter, new()
        {
            List<TDbParameter> result = null;
            var dbParameters 
                    = GetCachedStoreProcedureParameters
                        <TDbConnection, TDbCommand, TDbParameter>
                                (
                                    connectionString
                                    , storeProcedureName
                                    , onQueryDefinitionsSetInputParameterProcessFunc
                                    , onQueryDefinitionsSetReturnParameterProcessFunc
                                    , onQueryDefinitionsReadOneDbParameterProcessFunc
                                    , true
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
                                                        (
                                                            onExecutingSetDbParameterTypeProcessFunc
                                                        );
                    var parameterValue = onExecutingSetDbParameterValueProcessFunc(cloneDbParameter, jProperty.Value);
                    cloneDbParameter.Value = parameterValue;
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
                                                            (
                                                                onExecutingSetDbParameterTypeProcessFunc
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
        private static 
            ConcurrentDictionary<string, ExecutingInfo>
                _dictionary 
                    = new ConcurrentDictionary<string, ExecutingInfo>
                            (
                                StringComparer.OrdinalIgnoreCase
                            );
        public static void
                        RefreshCachedStoreProcedureExecuted
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

        public static
                IDictionary<string,DbParameter>
                    GetCachedStoreProcedureParameters
                        <TDbConnection, TDbCommand, TDbParameter>
                                (
                                    string connectionString
                                    , string storeProcedureName
                                    , Func<TDbParameter, TDbParameter>
                                            onQueryDefinitionsSetInputParameterProcessFunc
                                    , Func<TDbParameter, TDbParameter>
                                            onQueryDefinitionsSetReturnParameterProcessFunc
                                    , Func<IDataReader, TDbParameter, TDbParameter>
                                            onQueryDefinitionsReadOneDbParameterProcessFunc
                                    , bool includeReturnValueParameter = false
                                    //, int cacheExpireInSeconds = 0
                                )
                        where
                                TDbConnection : DbConnection ,new ()
                        where
                                TDbCommand : DbCommand, new()
                        where
                                TDbParameter : DbParameter, new()
        {
            ExecutingInfo GetExecutingInfo()
            {
                var dbParameters =
                        GetStoreProcedureParameters
                            <TDbConnection, TDbCommand, TDbParameter>
                                (
                                    connectionString
                                    , storeProcedureName
                                    , onQueryDefinitionsSetInputParameterProcessFunc
                                    , onQueryDefinitionsSetReturnParameterProcessFunc
                                    , onQueryDefinitionsReadOneDbParameterProcessFunc
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

        public static 
                IEnumerable<TDbParameter> 
                    GetStoreProcedureParameters
                        <TDbConnection, TDbCommand, TDbParameter>
                            (
                                string connectionString
                                , string storeProcedureName
                                , Func<TDbParameter, TDbParameter>
                                        onQueryDefinitionsSetInputParameterProcessFunc
                                , Func<TDbParameter, TDbParameter>
                                        onQueryDefinitionsSetReturnParameterProcessFunc
                                , Func<IDataReader, TDbParameter, TDbParameter>
                                        onQueryDefinitionsReadOneDbParameterProcessFunc
                                , bool includeReturnValueParameter = false
                            )
                    where
                        TDbConnection : DbConnection, new ()
                    where
                        TDbCommand : DbCommand, new()
                    where
                        TDbParameter : DbParameter, new()
        {
            DbConnection connection = null;
            try
            {
                connection = new TDbConnection();
                connection.ConnectionString = connectionString;
                var dataSource = connection.DataSource;
                var dataBase = connection.Database;
                string procedureSchema = string.Empty;
                string parameterName = string.Empty;
                var commandText = @"
                    SELECT
                        * 
                    FROM
                        information_schema.parameters a 
                    WHERE
                        a.SPECIFIC_NAME = @procedure_name
                    ";
                //MySQL 不支持 using command
                //using
                //    (
                DbCommand command = new TDbCommand()
                {
                    CommandText = commandText
                    ,
                    CommandType = CommandType.Text
                    ,
                    Connection = connection
                };
                    //)
                {
                    //command.CommandType = CommandType.StoredProcedure;
                    TDbParameter dbParameterProcedure_Name = new TDbParameter();
                    dbParameterProcedure_Name.ParameterName = "@procedure_name";
                    dbParameterProcedure_Name.Direction = ParameterDirection.Input;
                    dbParameterProcedure_Name.Size = 128;
                    dbParameterProcedure_Name
                            .Value = 
                                    (
                                        storeProcedureName != null 
                                        ?
                                        (object)storeProcedureName
                                        :
                                        DBNull.Value
                                    );

                    dbParameterProcedure_Name 
                            = onQueryDefinitionsSetInputParameterProcessFunc
                                    (
                                        dbParameterProcedure_Name
                                    );
                    command.Parameters.Add(dbParameterProcedure_Name);
                    TDbParameter dbParameterReturn = new TDbParameter();
                    dbParameterReturn.ParameterName = "@RETURN_VALUE";
                    dbParameterReturn.Direction = ParameterDirection.ReturnValue;
                    dbParameterReturn 
                        = onQueryDefinitionsSetReturnParameterProcessFunc
                                (
                                    dbParameterReturn
                                );
                    
                    connection.Open();
                    var dataReader
                            = command
                                    .ExecuteReader
                                        (
                                            CommandBehavior
                                                .CloseConnection
                                        );
                    var dbParameters
                            = dataReader
                                    .ExecuteRead
                                        (
                                            (x, reader) =>
                                            {
                                                var dbParameter = new TDbParameter();
                                                dbParameter
                                                    .ParameterName 
                                                        = (string)(reader["PARAMETER_NAME"]);
                                                if (reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value)
                                                {
                                                    dbParameter
                                                        .Size = reader
                                                                    .GetInt32
                                                                        (
                                                                            reader
                                                                                .GetOrdinal
                                                                                    ("CHARACTER_MAXIMUM_LENGTH")
                                                                        );
                                                }
                                                dbParameter
                                                    .Direction
                                                        = GetParameterDirection
                                                            (
                                                                reader
                                                                    .GetString
                                                                        (
                                                                            reader
                                                                                .GetOrdinal
                                                                                    ("PARAMETER_MODE")
                                                                        )
                                                            );
                                                var r =
                                                    onQueryDefinitionsReadOneDbParameterProcessFunc
                                                        (
                                                            reader
                                                            , dbParameter
                                                        );
                                                return r;
                                            }
                                        );
                    return dbParameters;
                }
            }
            finally
            {
                //connection.Close();
                //connection = null;
            }
        }
        private static ParameterDirection GetParameterDirection(string parameterMode)
        {
            ParameterDirection pd;
            if (string.Compare(parameterMode, "IN", true) == 0)
            {
                pd = ParameterDirection.Input;
            }
            else if (string.Compare(parameterMode, "INOUT", true) == 0)
            {
                pd = ParameterDirection.InputOutput;
            }
            else if (string.Compare(parameterMode, "RETURN", true) == 0)
            {
                pd = ParameterDirection.ReturnValue;
            }
            else
            {
                pd = ParameterDirection.Output;
            }
            return pd;
        }
        /// <summary>
        /// Converts the OleDb parameter direction
        /// </summary>
        /// <param name="oledbDirection">The integer parameter direction</param>
        /// <returns>A ParameterDirection</returns>
        private static ParameterDirection GetParameterDirection(short oledbDirection)
        {
            ParameterDirection pd;
            switch (oledbDirection)
            {
                case 1:
                    pd = ParameterDirection.Input;
                    break;
                case 2: //或者干脆注释掉 case 2 的全部
                    pd = ParameterDirection.Output; //是这里的问题
                    //2018 Microshoaft All Output will be used as InputOutput
                    goto default; //
                    //break; //我注释掉的这句话
                case 4:
                    pd = ParameterDirection.ReturnValue;
                    break;
                default:
                    pd = ParameterDirection.InputOutput;
                    break;
            }
            return pd;
        }

        public static JToken 
                    StoreProcedureExecute
                        <TDbConnection, TDbCommand, TDbParameter>
                                    (
                                        DbConnection connection
                                        , string storeProcedureName
                                        , Func<TDbParameter, TDbParameter>
                                                onQueryDefinitionsSetInputParameterProcessFunc
                                        , Func<TDbParameter, TDbParameter>
                                                onQueryDefinitionsSetReturnParameterProcessFunc
                                        , Func<IDataReader, TDbParameter, TDbParameter>
                                                onQueryDefinitionsReadOneDbParameterProcessFunc
                                        , Func<TDbParameter, TDbParameter, TDbParameter>
                                                onExecutingSetDbParameterTypeProcessFunc
                                        , Func<TDbParameter, JToken, object>
                                                onExecutingSetDbParameterValueProcessFunc
                                        , string p = null //string.Empty
                                        , int commandTimeout = 90
                                    )
                            where
                                    TDbConnection : DbConnection, new()
                            where
                                    TDbCommand : DbCommand, new()
                            where
                                    TDbParameter : DbParameter, new()
        {
            var inputsParameters = JObject.Parse(p);
            return
                StoreProcedureExecute
                    <TDbConnection, TDbCommand, TDbParameter>
                        (
                            connection
                            , storeProcedureName
                            , onQueryDefinitionsSetInputParameterProcessFunc
                            , onQueryDefinitionsSetReturnParameterProcessFunc
                            , onQueryDefinitionsReadOneDbParameterProcessFunc
                            , onExecutingSetDbParameterTypeProcessFunc
                            , onExecutingSetDbParameterValueProcessFunc
                            , inputsParameters
                            , commandTimeout
                        );
        }

        public static JToken
                    StoreProcedureExecute
                            <TDbConnection, TDbCommand, TDbParameter>
                                    (
                                        DbConnection connection
                                        , string storeProcedureName
                                        , Func<TDbParameter, TDbParameter>
                                                onQueryDefinitionsSetInputParameterProcessFunc
                                        , Func<TDbParameter, TDbParameter>
                                                onQueryDefinitionsSetReturnParameterProcessFunc
                                        , Func<IDataReader, TDbParameter, TDbParameter>
                                                onQueryDefinitionsReadOneDbParameterProcessFunc
                                        , Func<TDbParameter, TDbParameter, TDbParameter>
                                                onExecutingSetDbParameterTypeProcessFunc
                                        , Func<TDbParameter, JToken, object>
                                                onExecutingSetDbParameterValueProcessFunc
                                        , JToken inputsParameters = null //string.Empty
                                        , int commandTimeout = 90
                                    )
                            where
                                    TDbConnection : DbConnection , new()
                            where
                                    TDbCommand : DbCommand, new()
                            where
                                    TDbParameter : DbParameter, new()
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
                            = GenerateStoreProcedureExecuteParameters
                                    <TDbConnection, TDbCommand, TDbParameter>
                                        (
                                            connection.ConnectionString
                                            , storeProcedureName
                                            , inputsParameters
                                            , onQueryDefinitionsSetInputParameterProcessFunc
                                            , onQueryDefinitionsSetReturnParameterProcessFunc
                                            , onQueryDefinitionsReadOneDbParameterProcessFunc
                                            , onExecutingSetDbParameterTypeProcessFunc
                                            , onExecutingSetDbParameterValueProcessFunc
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