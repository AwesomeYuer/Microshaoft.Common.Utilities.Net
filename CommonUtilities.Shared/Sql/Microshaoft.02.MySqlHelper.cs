#if !XAMARIN
namespace Microshaoft
{
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    public static class MySqlHelper
    {
        public static int CachedExecutingParametersExpiredInSeconds
        {
            get;
            set;
        }

        public static List<MySqlParameter> GenerateExecuteMySqlParameters
                                (
                                    string connectionString
                                    , string storeProcedureName
                                    , JToken inputsParameters
                                )
        {
            List<MySqlParameter> result = null;
            var dbParameters = GetCachedStoreProcedureParameters
                                    (
                                        connectionString
                                        , storeProcedureName
                                        , true
                                    );
            var jProperties = (JObject)inputsParameters;
            foreach (KeyValuePair<string, JToken> jProperty in jProperties)
            {
                MySqlParameter sqlParameter = null;
                if
                    (
                        dbParameters
                            .TryGetValue
                                (
                                    jProperty.Key
                                    , out sqlParameter
                                )
                    )
                {
                    var direction = sqlParameter
                                        .Direction;
                    var cloneMySqlParameter = sqlParameter.ShallowClone();
                    cloneMySqlParameter.Value = (object)jProperty.Value;
                    if (result == null)
                    {
                        result = new List<MySqlParameter>();
                    }
                    result.Add(cloneMySqlParameter);
                }
            }
            foreach (var kvp in dbParameters)
            {
                var sqlParameter = kvp.Value;
                if (result == null)
                {
                    result = new List<MySqlParameter>();
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
                                                            , sqlParameter.ParameterName
                                                            , true
                                                        ) == 0
                                            );
                                    }
                                )
                    )
                {
                    var direction = sqlParameter.Direction;
                    if
                        (
                            direction != ParameterDirection.Input
                        )
                    {
                        if (result == null)
                        {
                            result = new List<MySqlParameter>();
                        }
                        var cloneMySqlParameter = sqlParameter.ShallowClone();
                        //if (direction == ParameterDirection.InputOutput)
                        //{
                        //    cloneMySqlParameter.Direction = ParameterDirection.Output;
                        //}
                        result.Add(cloneMySqlParameter);
                    }
                }
            }
            return result;
        }
        public static JValue GetJValue(MySqlParameter target)
        {
            JValue r = null;
            if
                (
                    target.MySqlDbType == MySqlDbType.VarChar
                    //||
                    //target.MySqlDbType == MySqlDbType.NVarChar
                    //||
                    //target.MySqlDbType == MySqlDbType.Char
                    //||
                    //target.MySqlDbType == MySqlDbType.NChar
                    ||
                    target.MySqlDbType == MySqlDbType.Text
                    //||
                    //target.MySqlDbType == MySqlDbType.NText
                    ||
                    target.MySqlDbType == MySqlDbType.VarString
                )
            {
                r = new JValue((string)target.Value);
            }
            else if
                (
                    target.MySqlDbType == MySqlDbType.DateTime
                    //||
                    //target.MySqlDbType == MySqlDbType.DateTime2
                    //||
                    //target.MySqlDbType == MySqlDbType.SmallDateTime
                    ||
                    target.MySqlDbType == MySqlDbType.Date
                    ||
                    target.MySqlDbType == MySqlDbType.DateTime
                )
            {
                r = new JValue((DateTime)target.Value);
            }
            //else if
            //    (
            //        target.MySqlDbType == MySqlDbType.DateTimeOffset
            //    )
            //{
            //    r = new JValue((DateTimeOffset)target.Value);
            //}
            else if
                (
                    target.MySqlDbType == MySqlDbType.Bit
                )
            {
                r = new JValue((bool)target.Value);
            }
            else if
                (
                    target.MySqlDbType == MySqlDbType.Decimal
                )
            {
                r = new JValue((decimal)target.Value);
            }
            else if
                (
                    target.MySqlDbType == MySqlDbType.Float
                )
            {
                r = new JValue((float)target.Value);
            }
            //else if
            //    (
            //        target.MySqlDbType == MySqlDbType.Real
            //    )
            //{
            //    r = new JValue((double)target.Value);
            //}
            else if
                (
                    target.MySqlDbType == MySqlDbType.Guid
                )
            {
                r = new JValue((Guid)target.Value);
            }
            else if
                (
                    target.MySqlDbType == MySqlDbType.UInt16
                    ||
                    target.MySqlDbType == MySqlDbType.UInt24
                    ||
                    target.MySqlDbType == MySqlDbType.UInt32
                    ||
                    target.MySqlDbType == MySqlDbType.UInt64
                    ||
                    target.MySqlDbType == MySqlDbType.Int16
                    ||
                    target.MySqlDbType == MySqlDbType.Int24
                    ||
                    target.MySqlDbType == MySqlDbType.Int32
                    ||
                    target.MySqlDbType == MySqlDbType.Int64
                )
            {
                r = new JValue((long)target.Value);
            }
            return r;
        }
        public static JArray ToJArray(this MySqlParameter[] target)
        {
            int i = 1;
            var result = new JArray();
            foreach (MySqlParameter parameter in target)
            {
                var jObject = new JObject();
                jObject.Add(nameof(parameter.ParameterName), new JValue(parameter.ParameterName));
                jObject.Add(nameof(parameter.MySqlDbType), new JValue(parameter.MySqlDbType.ToString()));
                jObject.Add(nameof(parameter.Size), new JValue(parameter.Size));
                jObject.Add(nameof(parameter.Direction), new JValue((long)parameter.Direction));
                jObject.Add(nameof(parameter.Scale), new JValue((long)parameter.Scale));
                jObject.Add(nameof(parameter.Precision), new JValue((long)parameter.Precision));
                result.Add(jObject);
                i++;
            }
            return result;
        }

        public static MySqlParameter ShallowClone(this MySqlParameter target, bool includeValue = false)
        {
            var result = new MySqlParameter();
            result.ParameterName = target.ParameterName;
            result.MySqlDbType = target.MySqlDbType;
            result.Size = target.Size;
            result.Direction = target.Direction;
            result.Scale = target.Scale;
            result.Precision = target.Precision;
            if (includeValue)
            {
                //Shadow copy
                result.Value = target.Value;
            }
            return result;
        }
        private class ExecutingInfo
        {
            public IDictionary<string, MySqlParameter> MySqlParameters;
            public DateTime RecentExecutedTime;
        }

        private static
            ConcurrentDictionary<string, ExecutingInfo>
                _dictionary
                    = new ConcurrentDictionary<string, ExecutingInfo>
                            (
                                StringComparer.OrdinalIgnoreCase
                            );



        //public static
        //    HashSet<string>
        //        StoreProceduresExecuteBlackList
        //            = null;


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
                executingInfo.RecentExecutedTime = DateTime.Now;
            }
        }

        public static
                IDictionary<string, MySqlParameter>
                        GetCachedStoreProcedureParameters
                                        (
                                            string connectionString
                                            , string storeProcedureName
                                            , bool includeReturnValueParameter = false
                                        //, int cacheExpireInSeconds = 0
                                        )
        {
            ExecutingInfo GetExecutingInfo()
            {
                var sqlParameters
                        = GetStoreProcedureParameters
                                (
                                    connectionString
                                    , storeProcedureName
                                    , includeReturnValueParameter
                                );
                var parameters =
                            sqlParameters
                                .ToDictionary
                                    (
                                        (xx) =>
                                        {
                                            return
                                                xx
                                                    .ParameterName
                                                    .TrimStart('@','?');
                                        }
                                        , StringComparer
                                                .OrdinalIgnoreCase
                                    );

                var _executingInfo = new ExecutingInfo()
                {
                    MySqlParameters = parameters,
                    RecentExecutedTime = DateTime.Now

                };
                return _executingInfo;
            }

            MySqlConnection connection = new MySqlConnection(connectionString);
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
            var result = executingInfo.MySqlParameters;
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
                        result = executingInfo.MySqlParameters;
                    }
                }
            }
            return result;
        }

        public static
                IEnumerable<MySqlParameter>
                        GetStoreProcedureParameters
                                        (
                                            string connectionString
                                            , string storeProcedureName
                                            , bool includeReturnValueParameter = false
                                        )
        {
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                var dataSource = connection.DataSource;
                var dataBase = connection.Database;
                //var key = $"{connection.DataSource}-{connection.Database}-{p_procedure_name}".ToUpper();
                //int groupNumber = 0;
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
                //commandText = "sp_procedure_params_rowset";



                //using
                //    (
                MySqlCommand command = new MySqlCommand(commandText, connection);
                //    )
                //{
                    //command.CommandType = CommandType.StoredProcedure;
                    MySqlParameter sqlParameterProcedure_Name = command.Parameters.Add("@procedure_name", MySqlDbType.VarChar, 128);
                    sqlParameterProcedure_Name.Value = (storeProcedureName != null ? (object)storeProcedureName : DBNull.Value);
                    //MySqlParameter sqlParameterGroup_Number = command.Parameters.Add("@group_number", MySqlDbType.Int);
                    //sqlParameterGroup_Number.Value = groupNumber;
                    //MySqlParameter sqlParameterProcedure_Schema = command.Parameters.Add("@procedure_schema", MySqlDbType.NVarChar, 128);
                    //sqlParameterProcedure_Schema.Value = (procedureSchema != null ? (object)procedureSchema : DBNull.Value);
                    //MySqlParameter sqlParameterParameter_Name = command.Parameters.Add("@parameter_name", MySqlDbType.NVarChar, 128);
                    //sqlParameterParameter_Name.Value = (parameterName != null ? (object)parameterName : DBNull.Value);
                    MySqlParameter sqlParameterReturn = command.Parameters.Add("@RETURN_VALUE", MySqlDbType.Int32);
                    sqlParameterReturn.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    IDataReader sqlDataReader = command
                                            .ExecuteReader
                                                (
                                                CommandBehavior.CloseConnection
                                                //CommandBehavior.CloseConnection
                                                );
                    var sqlParameters
                            = sqlDataReader
                                    .ExecuteRead<MySqlParameter>
                                        (
                                            (x, reader) =>
                                            {
                                                var sqlParameter = new MySqlParameter();
                                                sqlParameter
                                                    .ParameterName = "@" + (string)(reader["PARAMETER_NAME"]);
                                                var sqlDbTypeName = //(string)(reader["TYPE_NAME"]);
                                                                    (string)(reader["DATA_TYPE"]);
                                                MySqlDbType sqlDbType = (MySqlDbType)Enum.Parse(typeof(MySqlDbType), sqlDbTypeName, true);
                                                sqlParameter
                                                    .MySqlDbType = sqlDbType;
                                                if (reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value)
                                                {
                                                    sqlParameter
                                                        .Size = reader
                                                                    .GetInt32
                                                                        (
                                                                            reader
                                                                                .GetOrdinal("CHARACTER_MAXIMUM_LENGTH")
                                                                        );
                                                }
                                                sqlParameter
                                                        .Direction = GetParameterDirection
                                                                        (
                                                                            reader
                                                                                .GetString
                                                                                    (
                                                                                        reader
                                                                                            .GetOrdinal("PARAMETER_MODE")
                                                                                    )
                                                                        //reader
                                                                        //    .GetInt16
                                                                        //        (
                                                                        //            reader
                                                                        //                .GetOrdinal("PARAMETER_TYPE")
                                                                        //        )
                                                                        );
                                                if ((sqlParameter.MySqlDbType == MySqlDbType.Decimal))
                                                {
                                                    sqlParameter.Scale = (byte)(((short)(reader["NUMERIC_SCALE"]) & 255));
                                                    sqlParameter.Precision = (byte)(((short)(reader["NUMERIC_PRECISION"]) & 255));
                                                }
                                                return sqlParameter;
                                            }
                                        );
                    return sqlParameters;
                //}
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

        public static JObject StoreProcedureExecute
                               (
                                   MySqlConnection connection
                                   , string storeProcedureName
                                   , string p = null //string.Empty
                                   , int commandTimeout = 90
                               )
        {
            var inputsParameters = JObject.Parse(p);
            return
                StoreProcedureExecute
                        (
                            connection
                            , storeProcedureName
                            , inputsParameters
                            , commandTimeout
                        );
        }

        public static JObject StoreProcedureExecute
                                (
                                    MySqlConnection connection
                                    , string storeProcedureName
                                    , JToken inputsParameters = null //string.Empty
                                    , int commandTimeout = 90
                                )
        {
            var dataSource = connection.DataSource;
            var dataBaseName = connection.Database;
            try
            {
                //using
                //    (
                DbCommand command = new MySqlCommand
                                            (
                                                storeProcedureName
                                                , connection
                                            )
                {
                    CommandType = CommandType.StoredProcedure
                    ,
                    CommandTimeout = commandTimeout
                };
                    //)
                //{

                    var sqlParameters = MySqlHelper
                                            .GenerateExecuteMySqlParameters
                                                    (
                                                        connection.ConnectionString
                                                        , storeProcedureName
                                                        , inputsParameters
                                                    );
                    if (sqlParameters != null)
                    {
                        var parameters = sqlParameters.ToArray();
                        command.Parameters.AddRange(parameters);
                    }
                    connection.Open();
                    var result = new JObject
                    {
                        {
                            "TimeStamp"
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

                        ((JArray)result["Outputs"]["ResultSets"])
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
                    if (sqlParameters != null)
                    {
                        var outputParameters
                                = sqlParameters
                                        .Where
                                            (
                                                (x) =>
                                                {
                                                    return
                                                        (
                                                            x.Direction
                                                            !=
                                                            ParameterDirection.Input
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
                                        x.ParameterName.TrimStart('@','?')
                                        , new JValue(x.Value)
                                    );
                        }
                    }
                    if (jOutputParameters != null)
                    {
                        result["Outputs"]["Parameters"] = jOutputParameters;
                    }
                    return result;
                //}
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
#endif