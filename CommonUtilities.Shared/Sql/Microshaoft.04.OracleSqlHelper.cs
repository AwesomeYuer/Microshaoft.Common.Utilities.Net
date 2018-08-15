#if !XAMARIN && NETFRAMEWORK4_X
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    //using System.Data;
    using System.Data.SqlClient;
    using Oracle.ManagedDataAccess.Client;
    using System.Data;

    public static class OracleSqlHelper
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

        private static OracleParameter
                        onQueryDefinitionsSetInputParameterProcessFunc
                            (
                                OracleParameter parameter
                            )
        {
            parameter.OracleDbType = OracleDbType.Varchar2;
            return parameter;
        }
        private static OracleParameter
                        onQueryDefinitionsSetReturnParameterProcessFunc
                            (
                                OracleParameter parameter
                            )
        {
            parameter.OracleDbType = OracleDbType.Int32;
            return parameter;
        }
        private static OracleParameter
                       onQueryDefinitionsReadOneDbParameterProcessFunc
                           (
                               IDataReader reader
                               , OracleParameter parameter
                           )
        {
            var oracleSqlDbTypeName = //(string)(reader["TYPE_NAME"]);
                    (string)(reader["DATA_TYPE"]);
            OracleDbType oracleSqlDbType = (OracleDbType)Enum.Parse(typeof(OracleDbType), oracleSqlDbTypeName, true);
            parameter
                .OracleDbType = oracleSqlDbType;
            if ((parameter.OracleDbType == OracleDbType.Decimal))
            {
                parameter.Scale = (byte)(((short)(reader["NUMERIC_SCALE"]) & 255));
                parameter.Precision = (byte)(((short)(reader["NUMERIC_PRECISION"]) & 255));
            }
            return parameter;
        }
        private static OracleParameter
                       onExecutingSetDbParameterTypeProcessFunc
                            (
                                OracleParameter definitionOracleParameter
                                , OracleParameter cloneOracleParameter
                            )
        {
            //to do
            cloneOracleParameter.OracleDbType = definitionOracleParameter.OracleDbType;
            return cloneOracleParameter;
        }

        private static object
               onExecutingSetDbParameterValueProcessFunc
                    (
                        OracleParameter parameter
                        , JToken jValue
                    )
        {
            object r = null;
            var jValueText = jValue.ToString();
            if
                    (
                       parameter.OracleDbType == OracleDbType.Varchar2
                       ||
                       parameter.OracleDbType == OracleDbType.NVarchar2
                       ||
                       parameter.OracleDbType == OracleDbType.Char
                       ||
                       parameter.OracleDbType == OracleDbType.NChar
                       //||
                       //parameter.OracleDbType == OracleDbType.Text
                       //||
                       //parameter.OracleDbType == OracleDbType.NText
                    )
            {
                r = jValueText;
            }
            else if
                (
                    parameter.OracleDbType == OracleDbType.Date
                    ||
                    parameter.OracleDbType == OracleDbType.TimeStamp
                    ||
                    parameter.OracleDbType == OracleDbType.TimeStampLTZ
                    ||
                    parameter.OracleDbType == OracleDbType.TimeStampTZ
                )
            {
                r = DateTime.Parse(jValueText);
            }
            else if
                (
                    parameter.OracleDbType == OracleDbType.Boolean
                )
            {
                r = bool.Parse(jValueText);
            }
            else if
                (
                    parameter.OracleDbType == OracleDbType.Decimal
                )
            {
                r = decimal.Parse(jValueText);
            }
            else if
                (
                    parameter.OracleDbType == OracleDbType.Double
                )
            {
                r = double.Parse(jValueText);
            }
            else if
                (
                    parameter.OracleDbType == OracleDbType.Raw
                )
            {
                r = Guid.Parse(jValueText);
            }
            else if
                (
                    parameter.OracleDbType == OracleDbType.Long
                    ||
                    parameter.OracleDbType == OracleDbType.Int64
                )
            {
                r = long.Parse(jValueText);
            }
            else if
                (
                    parameter.OracleDbType == OracleDbType.Int32
                )
            {
                r = int.Parse(jValueText);
            }
            else if
                (
                    parameter.OracleDbType == OracleDbType.Int64
                )
            {
                r = short.Parse(jValueText);
            }
            return r;

        }

        public static List<OracleParameter> GenerateExecuteParameters
                                (
                                    string connectionString
                                    , string storeProcedureName
                                    , JToken inputsParameters
                                )
        {
            var result
                    = SqlHelper
                        .GenerateStoreProcedureExecuteParameters
                            <OracleConnection, OracleCommand, OracleParameter>
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
                                   OracleConnection connection
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
                                    OracleConnection connection
                                    , string storeProcedureName
                                    , JToken inputsParameters = null //string.Empty
                                    , int commandTimeout = 90
                                )
        {
            var r = SqlHelper
                        .StoreProcedureExecute
                            <OracleConnection, OracleCommand, OracleParameter>
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
                                    OracleConnection connection
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