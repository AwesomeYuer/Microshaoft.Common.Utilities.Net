namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;

    public static class SqlHelper
    {
        public static List<SqlParameter> GenerateExecuteSqlParameters
                                (
                                    string connectionString
                                    , string storeProcedureName
                                    , JObject actualParameters
                                )
        {
            List<SqlParameter> result = null;
            var sqlParameters = GetCachedStoreProcedureParameters
                                    (
                                        connectionString
                                        , storeProcedureName
                                        , true
                                    );
            foreach (KeyValuePair<string, JToken> jProperty in actualParameters)
            {
                SqlParameter sqlParameter = null;
                if
                    (
                        sqlParameters
                            .TryGetValue
                                (
                                    jProperty.Key
                                    , out sqlParameter
                                )
                    )
                {
                    var direction = sqlParameter
                                        .Direction;
                    if 
                        (
                            direction == ParameterDirection.Input
                            ||
                            direction == ParameterDirection.InputOutput
                        )
                    {
                        var r = sqlParameter.ShallowClone();
                        r.Value = (object)jProperty.Value;
                        if (result == null)
                        {
                            result = new List<SqlParameter>();
                        }
                        result.Add(r);
                    }
                }
            }
            
  

            foreach (var kvp in sqlParameters)
            {
                var sqlParameter = kvp.Value;
                if 
                    (
                        !result
                            .Exists
                                (
                                    (x) =>
                                    {
                                        return
                                            (string.Compare(x.ParameterName, sqlParameter.ParameterName, true) == 0);
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
                            result = new List<SqlParameter>();
                        }
                        result.Add(sqlParameter.ShallowClone());
                    }

                }
                
                
            }

            return result;

        }
        public static JValue GetJValue(this SqlParameter target)
        {
            JValue r = null;
            if
                (
                    target.SqlDbType == SqlDbType.VarChar
                    ||
                    target.SqlDbType == SqlDbType.NVarChar
                    ||
                    target.SqlDbType == SqlDbType.Char
                    ||
                    target.SqlDbType == SqlDbType.NChar
                    ||
                    target.SqlDbType == SqlDbType.Text
                    ||
                    target.SqlDbType == SqlDbType.NText
                )
            {
                r = new JValue((string)target.Value);
            }
            else if
                (
                    target.SqlDbType == SqlDbType.DateTime
                    ||
                    target.SqlDbType == SqlDbType.DateTime2
                    ||
                    target.SqlDbType == SqlDbType.SmallDateTime
                    ||
                    target.SqlDbType == SqlDbType.Date
                    ||
                    target.SqlDbType == SqlDbType.DateTime
                )
            {
                r = new JValue((DateTime)target.Value);
            }
            else if
                (
                    target.SqlDbType == SqlDbType.DateTimeOffset
                )
            {
                r = new JValue((DateTimeOffset)target.Value);
            }
            else if
                (
                    target.SqlDbType == SqlDbType.Bit
                    
                )
            {
                r = new JValue((bool)target.Value);
            }
            else if
                (
                    target.SqlDbType == SqlDbType.Decimal

                )
            {
                r = new JValue((decimal)target.Value);
            }
            else if
                (
                    target.SqlDbType == SqlDbType.Float

                )
            {
                r = new JValue((float)target.Value);
            }
            else if
                (
                    target.SqlDbType == SqlDbType.Real

                )
            {
                r = new JValue((double)target.Value);
            }
            else if
                (
                    target.SqlDbType == SqlDbType.UniqueIdentifier
                )
            {
                r = new JValue((Guid)target.Value);
            }
            else if
                (
                    target.SqlDbType == SqlDbType.BigInt
                    ||
                    target.SqlDbType == SqlDbType.Int
                    ||
                    target.SqlDbType == SqlDbType.SmallInt
                    ||
                    target.SqlDbType == SqlDbType.TinyInt
                )
            {
                r = new JValue((long)target.Value);
            }
            return r;
        }
        public static JArray ToJArray(this SqlParameter[] target)
                        
        {
            int i = 1;
            var result = new JArray();
            foreach (SqlParameter parameter in target)
            {
                var jObject = new JObject();
                jObject.Add(nameof(parameter.ParameterName), new JValue(parameter.ParameterName));
                jObject.Add(nameof(parameter.SqlDbType), new JValue(parameter.SqlDbType.ToString()));
                jObject.Add(nameof(parameter.Size), new JValue(parameter.Size));
                jObject.Add(nameof(parameter.Direction), new JValue((long)parameter.Direction));
                jObject.Add(nameof(parameter.Scale), new JValue((long) parameter.Scale));
                jObject.Add(nameof(parameter.Precision), new JValue((long)parameter.Precision));
                result.Add(jObject);
                i++;
            }
            return result;

        }

        public static SqlParameter ShallowClone(this SqlParameter target, bool includeValue = false)
        {
            var result = new SqlParameter();
            result.ParameterName = target.ParameterName;
            result.SqlDbType = target.SqlDbType;
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


        public static ConcurrentDictionary<string, IDictionary<string, SqlParameter>>
            _dictionary 
                = new ConcurrentDictionary<string, IDictionary<string, SqlParameter>>
                            (
                                StringComparer.OrdinalIgnoreCase
                            );
        public static IDictionary<string,SqlParameter> GetCachedStoreProcedureParameters
                                        (
                                            string connectionString
                                            , string storeProcedureName
                                            , bool includeReturnValueParameter = false
                                        )
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var key = $"{connection.DataSource}-{connection.Database}-{storeProcedureName}".ToUpper();
            var result = _dictionary
                                .GetOrAdd
                                        (
                                            key
                                            , (x) =>
                                            {
                                                var sqlParameters
                                                        = GetStoreProcedureParameters
                                                                (
                                                                    connectionString
                                                                    , storeProcedureName
                                                                    , includeReturnValueParameter
                                                                );
                                                var r = sqlParameters
                                                                .ToDictionary
                                                                    (
                                                                        (xx) =>
                                                                        {
                                                                            return xx
                                                                                    .ParameterName
                                                                                    .TrimStart('@');
                                                                        }
                                                                        , StringComparer.OrdinalIgnoreCase
                                                                    );
                                                return r;
                                            }
                                        );
            return result;
        }

        public static IEnumerable<SqlParameter> GetStoreProcedureParameters
                                        (
                                            string connectionString
                                            , string storeProcedureName
                                            , bool includeReturnValueParameter = false
                                        )
        {
            SqlConnection connection = null;

            try
            {
                connection = new SqlConnection(connectionString);
                var dataSource = connection.DataSource;
                var dataBase = connection.Database;
                //var key = $"{connection.DataSource}-{connection.Database}-{p_procedure_name}".ToUpper();
                int groupNumber = 0;
                string procedureSchema = string.Empty;
                string parameterName = string.Empty;

                using (SqlCommand command = new SqlCommand("sp_procedure_params_rowset", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;


                    SqlParameter sqlParameterProcedure_Name = command.Parameters.Add("@procedure_name", SqlDbType.NVarChar, 128);
                    sqlParameterProcedure_Name.Value = (storeProcedureName != null ? (object)storeProcedureName : DBNull.Value);
                    //SqlParameter sqlParameterGroup_Number = command.Parameters.Add("@group_number", SqlDbType.Int);
                    //sqlParameterGroup_Number.Value = groupNumber;
                    //SqlParameter sqlParameterProcedure_Schema = command.Parameters.Add("@procedure_schema", SqlDbType.NVarChar, 128);
                    //sqlParameterProcedure_Schema.Value = (procedureSchema != null ? (object)procedureSchema : DBNull.Value);
                    //SqlParameter sqlParameterParameter_Name = command.Parameters.Add("@parameter_name", SqlDbType.NVarChar, 128);
                    //sqlParameterParameter_Name.Value = (parameterName != null ? (object)parameterName : DBNull.Value);
                    SqlParameter sqlParameterReturn = command.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
                    sqlParameterReturn.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    var sqlDataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                    var sqlParameters
                            = sqlDataReader
                                    .ExecuteRead<SqlParameter>
                                        (
                                            (x, reader) =>
                                            {
                                                var sqlParameter = new SqlParameter();
                                                sqlParameter
                                                    .ParameterName = (string)(reader["PARAMETER_NAME"]);
                                                var sqlDbTypeName = (string)(reader["TYPE_NAME"]);
                                                SqlDbType sqlDbType = (SqlDbType) Enum.Parse(typeof(SqlDbType), sqlDbTypeName, true);
                                                
                                                sqlParameter
                                                    .SqlDbType = sqlDbType;
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
                                                                                .GetInt16
                                                                                    (
                                                                                        reader
                                                                                            .GetOrdinal("PARAMETER_TYPE")
                                                                                    )
                                                                        );
                                                if ((sqlParameter.SqlDbType == SqlDbType.Decimal))
                                                {
                                                    sqlParameter.Scale = (byte)(((short)(reader["NUMERIC_SCALE"]) & 255));
                                                    sqlParameter.Precision = (byte)(((short)(reader["NUMERIC_PRECISION"]) & 255));
                                                }
                                                return sqlParameter;
                                            }
                                        );
                    return sqlParameters;
                }
                
            }
            finally
            {
                //connection.Close();
                //connection = null;
            }
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
                    goto default; //我加的这句话
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
        

    }


}
