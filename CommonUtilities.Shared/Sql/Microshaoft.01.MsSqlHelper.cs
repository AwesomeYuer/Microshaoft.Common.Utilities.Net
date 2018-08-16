namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
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
            var originalDbTypeName = (string)(reader["DATA_TYPE"]);
            var dbTypeName = originalDbTypeName;
            if (string.Compare(dbTypeName, "sql_variant", true) == 0)
            {
                dbTypeName = "variant";
            }
            else if (string.Compare(dbTypeName, "numeric", true) == 0)
            {
                dbTypeName = "decimal";
            }
            else if (string.Compare(dbTypeName, "hierarchyid", true) == 0)
            {
                dbTypeName = "int";
            }
            SqlDbType dbType = SqlDbType.Udt;
            var r =  Enum
                        .TryParse
                            (
                                dbTypeName
                                , true
                                , out dbType
                            );
            if (r)
            {
                parameter
                    .SqlDbType = dbType;
            }
            if ((parameter.SqlDbType == SqlDbType.Decimal))
            {
                parameter
                    .Scale =
                        (
                            (byte)
                                (
                                    (
                                        (short)
                                            (
                                                (int)(reader["NUMERIC_SCALE"])
                                            )
                                    )
                                    //& 255
                                )
                        );
                parameter.Precision = ((byte)reader["NUMERIC_PRECISION"]);
            }
            else if (parameter.SqlDbType == SqlDbType.Udt)
            {
                //, @geometry geometry = null
                //, @geography geography = null
                parameter.UdtTypeName = originalDbTypeName;
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
            var jValueText = jValue.ToString();
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
                r = jValueText;
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
                r = DateTime.Parse(jValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.DateTimeOffset
                )
            {
                r = DateTimeOffset.Parse(jValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Bit
                )
            {
                r = bool.Parse(jValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Decimal
                )
            {
                r = decimal.Parse(jValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Float
                )
            {
                r = float.Parse(jValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Real
                )
            {
                r = double.Parse(jValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.UniqueIdentifier
                )
            {
                r = Guid.Parse(jValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.BigInt
                )
            {
                r = long.Parse(jValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Int
                )
            {
                r = int.Parse(jValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.SmallInt
                )
            {
                r = short.Parse(jValueText);
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.TinyInt
                )
            {
                r = short.Parse(jValueText);
            }
            return r;
        }
        public static List<SqlParameter> GenerateExecuteParameters
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
                                );
            return result;
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