#if !NETFRAMEWORK4_5_X && !XAMARIN
namespace Microshaoft
{
    using Microsoft.Data.Sqlite;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    public static class SqliteHelper
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

        private static SqliteParameter
                        onQueryDefinitionsSetInputParameterProcessFunc
                            (
                                SqliteParameter parameter
                            )
        {
            parameter.SqliteType = SqliteType.Text;
            return parameter;
        }
        private static SqliteParameter
                        onQueryDefinitionsSetReturnParameterProcessFunc
                            (
                                SqliteParameter parameter
                            )
        {
            parameter.SqliteType = SqliteType.Integer;
            return parameter;
        }
        private static SqliteParameter
                       onQueryDefinitionsReadOneDbParameterProcessFunc
                           (
                               IDataReader reader
                               , SqliteParameter parameter
                           )
        {
            var dbTypeName = (string)(reader["DATA_TYPE"]);
            SqliteType dbType = (SqliteType)Enum.Parse(typeof(SqliteType), dbTypeName, true);
            parameter
                .SqliteType = dbType;
            if ((parameter.SqliteType == SqliteType.Real))
            {
                parameter.Scale = (byte)(((short)(reader["NUMERIC_SCALE"]) & 255));
                parameter.Precision = (byte)(((short)(reader["NUMERIC_PRECISION"]) & 255));
            }
            return parameter;
        }
        private static SqliteParameter
                       onExecutingSetDbParameterTypeProcessFunc
                            (
                                SqliteParameter definitionSqliteParameter
                                , SqliteParameter cloneSqliteParameter
                            )
        {
            cloneSqliteParameter
                    .SqliteType = definitionSqliteParameter.SqliteType;
            return cloneSqliteParameter;
        }

        private static object
               onExecutingSetDbParameterValueProcessFunc
                    (
                        SqliteParameter parameter
                        , JToken jValue
                    )
        {
            object r = null;
            var jValueText = jValue.ToString();
            if
                (
                    parameter.SqliteType == SqliteType.Text
                )
            {
                r = jValueText;
            }
            else if
                (
                    parameter.SqliteType == SqliteType.Real
                )
            {
                r = decimal.Parse(jValueText);
            }
            else if
                (
                    parameter.SqliteType == SqliteType.Real
                )
            {
                r = double.Parse(jValueText);
            }
            else if
                (
                    parameter.SqliteType == SqliteType.Integer
                )
            {
                r = int.Parse(jValueText);
            }
            return r;
        }
        public static List<SqliteParameter> GenerateExecuteParameters
                                (
                                    string connectionString
                                    , string storeProcedureName
                                    , JToken inputsParameters
                                )
        {
            var result
                    = SqlHelper
                        .GenerateStoreProcedureExecuteParameters
                            <SqliteConnection, SqliteCommand, SqliteParameter>
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
                                   SqliteConnection connection
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
                                    SqliteConnection connection
                                    , string storeProcedureName
                                    , JToken inputsParameters = null //string.Empty
                                    , int commandTimeout = 90
                                )
        {
            var r = SqlHelper
                        .StoreProcedureExecute
                            <SqliteConnection, SqliteCommand, SqliteParameter>
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
                                    SqliteConnection connection
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