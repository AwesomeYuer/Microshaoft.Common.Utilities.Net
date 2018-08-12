namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    public static class MsSqlHelper
    {
        public static int CachedExecutingParametersExpiredInSeconds
        {
            get
            {
                return
                    SqlHelper
                        .CachedExecutingParametersExpiredInSeconds;
            }
            set
            {
                SqlHelper
                    .CachedExecutingParametersExpiredInSeconds = value;
            }
        }

        private static SqlParameter
                        onQueryDefinitionsSetInputParameterProcessFunc
                            (
                                SqlParameter parameter
                            )
        {
            parameter.SqlDbType = SqlDbType.NVarChar;
            return parameter;
        }
        private static SqlParameter
                        onQueryDefinitionsSetReturnParameterProcessFunc
                            (
                                SqlParameter parameter
                            )
        {
            parameter.SqlDbType = SqlDbType.Int;
            return parameter;
        }
        private static SqlParameter
                       onQueryDefinitionsReadOneDbParameterProcessFunc
                           (
                               IDataReader reader
                               , SqlParameter parameter
                           )
        {
            var sqlDbTypeName = //(string)(reader["TYPE_NAME"]);
                    (string)(reader["DATA_TYPE"]);
            SqlDbType sqlDbType = (SqlDbType)Enum.Parse(typeof(SqlDbType), sqlDbTypeName, true);
            parameter
                .SqlDbType = sqlDbType;
            if ((parameter.SqlDbType == SqlDbType.Decimal))
            {
                parameter.Scale = (byte)(((short)(reader["NUMERIC_SCALE"]) & 255));
                parameter.Precision = (byte)(((short)(reader["NUMERIC_PRECISION"]) & 255));
            }
            return parameter;
        }
        private static SqlParameter
                       onExecutingSetDbParameterTypeProcessFunc
                            (
                                SqlParameter definitionSqlParameter
                                , SqlParameter cloneSqlParameter
                            )
        {
            //to do
            cloneSqlParameter.SqlDbType = definitionSqlParameter.SqlDbType;
            return cloneSqlParameter;
        }

        private static object
               onExecutingSetDbParameterValueProcessFunc
                    (
                        SqlParameter parameter
                        , JToken jValue
                    )
        {
            object r = null;
            var parameterValueText = jValue.ToString();
            if
                    (
                       parameter.SqlDbType == SqlDbType.VarChar
                       ||
                       parameter.SqlDbType == SqlDbType.NVarChar
                       ||
                       parameter.SqlDbType == SqlDbType.Char
                       ||
                       parameter.SqlDbType == SqlDbType.NChar
                       ||
                       parameter.SqlDbType == SqlDbType.Text
                       ||
                       parameter.SqlDbType == SqlDbType.NText
                    )
            {
                r = parameterValueText;
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.DateTime
                    ||
                    parameter.SqlDbType == SqlDbType.DateTime2
                    ||
                    parameter.SqlDbType == SqlDbType.SmallDateTime
                    ||
                    parameter.SqlDbType == SqlDbType.Date
                    ||
                    parameter.SqlDbType == SqlDbType.DateTime
                )
            {
                r = DateTime.Parse(parameterValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.DateTimeOffset
                )
            {
                r = DateTimeOffset.Parse(parameterValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Bit
                )
            {
                r = bool.Parse(parameterValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Decimal
                )
            {
                r = decimal.Parse(parameterValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Float
                )
            {
                r = float.Parse(parameterValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Real
                )
            {
                r = double.Parse(parameterValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.UniqueIdentifier
                )
            {
                r = Guid.Parse(parameterValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.BigInt
                )
            {
                r = long.Parse(parameterValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Int
                )
            {
                r = int.Parse(parameterValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.SmallInt
                )
            {
                r = short.Parse(parameterValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.TinyInt
                )
            {
                r = short.Parse(parameterValueText);
            }
            return r;

        }

        public static List<SqlParameter> GenerateExecuteSqlParameters
                                (
                                    string connectionString
                                    , string storeProcedureName
                                    , JToken inputsParameters
                                )
        {
            var result
                    = SqlHelper
                        .GenerateStoreProcedureExecuteParameters
                            <SqlConnection, SqlCommand, SqlParameter>
                                (
                                    connectionString
                                    , storeProcedureName
                                    , inputsParameters
                                    , onQueryDefinitionsSetInputParameterProcessFunc
                                    , onQueryDefinitionsSetReturnParameterProcessFunc
                                    , onQueryDefinitionsReadOneDbParameterProcessFunc
                                    , onExecutingSetDbParameterTypeProcessFunc
                                    , onExecutingSetDbParameterValueProcessFunc
                                )
                        .Select
                            (
                                (x) =>
                                {
                                    return
                                        (SqlParameter)x;
                                }
                            )
                        .ToList();
            return result;
        }
        private static JValue GetJValue(this SqlParameter target)
        {
            JValue processFunc(SqlParameter parameter)
            {
                JValue rr = null;
                if
                    (
                       parameter.SqlDbType == SqlDbType.VarChar
                       ||
                       parameter.SqlDbType == SqlDbType.NVarChar
                       ||
                       parameter.SqlDbType == SqlDbType.Char
                       ||
                       parameter.SqlDbType == SqlDbType.NChar
                       ||
                       parameter.SqlDbType == SqlDbType.Text
                       ||
                       parameter.SqlDbType == SqlDbType.NText
                    )
                {
                    rr = new JValue((string)parameter.Value);
                }
                else if
                    (
                        parameter.SqlDbType == SqlDbType.DateTime
                        ||
                        parameter.SqlDbType == SqlDbType.DateTime2
                        ||
                        parameter.SqlDbType == SqlDbType.SmallDateTime
                        ||
                        parameter.SqlDbType == SqlDbType.Date
                        ||
                        parameter.SqlDbType == SqlDbType.DateTime
                    )
                {
                    rr = new JValue((DateTime)parameter.Value);
                }
                else if
                    (
                        parameter.SqlDbType == SqlDbType.DateTimeOffset
                    )
                {
                    rr = new JValue((DateTimeOffset)parameter.Value);
                }
                else if
                    (
                        parameter.SqlDbType == SqlDbType.Bit
                    )
                {
                    rr = new JValue((bool)parameter.Value);
                }
                else if
                    (
                        parameter.SqlDbType == SqlDbType.Decimal
                    )
                {
                    rr = new JValue((decimal)parameter.Value);
                }
                else if
                    (
                        parameter.SqlDbType == SqlDbType.Float
                    )
                {
                    rr = new JValue((float)parameter.Value);
                }
                else if
                    (
                        parameter.SqlDbType == SqlDbType.Real
                    )
                {
                    rr = new JValue((double)parameter.Value);
                }
                else if
                    (
                        parameter.SqlDbType == SqlDbType.UniqueIdentifier
                    )
                {
                    rr = new JValue((Guid)parameter.Value);
                }
                else if
                    (
                        parameter.SqlDbType == SqlDbType.BigInt
                        ||
                        parameter.SqlDbType == SqlDbType.Int
                        ||
                        parameter.SqlDbType == SqlDbType.SmallInt
                        ||
                        parameter.SqlDbType == SqlDbType.TinyInt
                    )
                {
                    rr = new JValue((long)parameter.Value);
                }
                return rr;
            }
            JValue r = null;
            r = SqlHelper
                    .GetJValue<SqlParameter>
                        (
                            target
                            , processFunc
                        );
            return r;
        }
        
        private static
                IDictionary<string,DbParameter>
                        GetCachedStoreProcedureParameters
                                        (
                                            string connectionString
                                            , string storeProcedureName
                                            , bool includeReturnValueParameter = false
                                            //, int cacheExpireInSeconds = 0
                                        )
        {
            var r =
                    SqlHelper
                        .GetCachedStoreProcedureParameters
                            <SqlConnection, SqlCommand, SqlParameter>
                                (
                                    connectionString
                                    , storeProcedureName
                                    , onQueryDefinitionsSetInputParameterProcessFunc
                                    , onQueryDefinitionsSetReturnParameterProcessFunc
                                    , onQueryDefinitionsReadOneDbParameterProcessFunc
                                    , includeReturnValueParameter
                                );
            return r;
        }

        private static 
                IEnumerable<DbParameter> 
                        GetStoreProcedureParameters
                                        (
                                            string connectionString
                                            , string storeProcedureName
                                            , bool includeReturnValueParameter = false
                                        )
        {
            var r =
                    SqlHelper
                        .GetStoreProcedureParameters
                            <SqlConnection, SqlCommand, SqlParameter>
                            (
                                connectionString
                                , storeProcedureName
                                , onQueryDefinitionsSetInputParameterProcessFunc
                                , onQueryDefinitionsSetReturnParameterProcessFunc
                                , onQueryDefinitionsReadOneDbParameterProcessFunc
                                , includeReturnValueParameter

                            );
            return r;
        }
       
        public static JToken StoreProcedureExecute
                               (
                                   SqlConnection connection
                                   , string storeProcedureName
                                   , string p = null //string.Empty
                                   , int commandTimeout = 90
                               )
        {
            JToken inputsParameters = JObject.Parse(p);
            return
                StoreProcedureExecute
                        (
                            connection
                            , storeProcedureName
                            , inputsParameters
                            , commandTimeout
                        );
        }

        public static JToken StoreProcedureExecute
                                (
                                    SqlConnection connection
                                    , string storeProcedureName
                                    , JToken inputsParameters = null //string.Empty
                                    , int commandTimeout = 90
                                )
        {
            var r = SqlHelper
                        .StoreProcedureExecute
                            <SqlConnection, SqlCommand, SqlParameter>
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
            return r;
        }
        public static void
                RefreshCachedStoreProcedureExecuted
                                (
                                    SqlConnection connection
                                    , string storeProcedureName
                                )
        {
            SqlHelper
                    .RefreshCachedStoreProcedureExecuted
                            (
                                connection
                                , storeProcedureName
                            );
        }
    }
}