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

        private static MySqlParameter
                        onQueryDefinitionsSetInputParameterProcessFunc
                            (
                                MySqlParameter parameter
                            )
        {
            parameter.MySqlDbType = MySqlDbType.VarChar;
            return parameter;
        }
        private static MySqlParameter
                        onQueryDefinitionsSetReturnParameterProcessFunc
                            (
                                MySqlParameter parameter
                            )
        {
            parameter.MySqlDbType = MySqlDbType.Int32;
            return parameter;
        }
        private static MySqlParameter
                       onQueryDefinitionsReadOneDbParameterProcessFunc
                           (
                               IDataReader reader
                               , MySqlParameter parameter
                           )
        {
            var mySqlDbTypeName = //(string)(reader["TYPE_NAME"]);
                    (string)(reader["DATA_TYPE"]);
            MySqlDbType sqlDbType = (MySqlDbType)Enum.Parse(typeof(MySqlDbType), mySqlDbTypeName, true);
            parameter
                .MySqlDbType = sqlDbType;
            if ((parameter.MySqlDbType == MySqlDbType.Decimal))
            {
                parameter.Scale = (byte)(((short)(reader["NUMERIC_SCALE"]) & 255));
                parameter.Precision = (byte)(((short)(reader["NUMERIC_PRECISION"]) & 255));
            }
            return parameter;
        }
        private static MySqlParameter
                       onExecutingSetDbParameterTypeProcessFunc
                            (
                                MySqlParameter definitionMySqlParameter
                                , MySqlParameter cloneMySqlParameter
                            )
        {
            //to do
            cloneMySqlParameter.MySqlDbType = definitionMySqlParameter.MySqlDbType;
            return cloneMySqlParameter;
        }

        private static object
               onExecutingSetDbParameterValueProcessFunc
                    (
                        MySqlParameter parameter
                        , JToken jValue
                    )
        {
            object r = null;
            var jValueText = jValue.ToString();

            if
                (
                    parameter.MySqlDbType == MySqlDbType.VarChar
                    //||
                    //parameter.MySqlDbType == MySqlDbType.NVarChar
                    //||
                    //parameter.MySqlDbType == MySqlDbType.Char
                    //||
                    //parameter.MySqlDbType == MySqlDbType.NChar
                    ||
                    parameter.MySqlDbType == MySqlDbType.Text
                    //||
                    //target.MySqlDbType == MySqlDbType.NText
                    ||
                    parameter.MySqlDbType == MySqlDbType.VarString
                )
            {
                r = jValueText;
            }
            else if
                (
                    parameter.MySqlDbType == MySqlDbType.DateTime
                    //||
                    //parameter.MySqlDbType == MySqlDbType.DateTime2
                    //||
                    //parameter.MySqlDbType == MySqlDbType.SmallDateTime
                    ||
                    parameter.MySqlDbType == MySqlDbType.Date
                    ||
                    parameter.MySqlDbType == MySqlDbType.DateTime
                )
            {
                r = DateTime.Parse(jValueText);
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
                    parameter.MySqlDbType == MySqlDbType.Bit
                )
            {
                r = bool.Parse(jValueText);
            }
            else if
                (
                    parameter.MySqlDbType == MySqlDbType.Decimal
                )
            {
                r = decimal.Parse(jValueText);
            }
            else if
                (
                    parameter.MySqlDbType == MySqlDbType.Float
                )
            {
                r = float.Parse(jValueText);
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
                    parameter.MySqlDbType == MySqlDbType.Guid
                )
            {
                r = Guid.Parse(jValueText);
            }
            else if
                (
                    parameter.MySqlDbType == MySqlDbType.UInt16
                )
            {
                r = ushort.Parse(jValueText);
            }
            else if
                (
                    parameter.MySqlDbType == MySqlDbType.UInt24
                    ||
                    parameter.MySqlDbType == MySqlDbType.UInt32
                )
            {
                r = uint.Parse(jValueText);
            }
            else if
                (
                    parameter.MySqlDbType == MySqlDbType.UInt64
                )
            {
                r = ulong.Parse(jValueText);
            }
            else if
               (
                   parameter.MySqlDbType == MySqlDbType.Int16
               )
            {
                r = short.Parse(jValueText);
            }
            else if
               (
                    parameter.MySqlDbType == MySqlDbType.Int24
                    ||
                    parameter.MySqlDbType == MySqlDbType.Int32
               )
            {
                r = int.Parse(jValueText);

            }
            else if
               (
                    parameter.MySqlDbType == MySqlDbType.Int64
               )
            {
                r = long.Parse(jValueText);
            }
            return r;

        }

        public static List<MySqlParameter> GenerateExecuteParameters
                                (
                                    string connectionString
                                    , string storeProcedureName
                                    , JToken inputsParameters
                                )
        {
            var result
                    = SqlHelper
                        .GenerateStoreProcedureExecuteParameters
                            <MySqlConnection, MySqlCommand, MySqlParameter>
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
                                   MySqlConnection connection
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
                                    MySqlConnection connection
                                    , string storeProcedureName
                                    , JToken inputsParameters = null //string.Empty
                                    , int commandTimeout = 90
                                )
        {
            var r = SqlHelper
                        .StoreProcedureExecute
                            <MySqlConnection, MySqlCommand, MySqlParameter>
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
                                    MySqlConnection connection
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
#endif