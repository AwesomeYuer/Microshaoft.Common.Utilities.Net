namespace Microshaoft
{
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using Npgsql;
    using Oracle.ManagedDataAccess.Client;
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;
    public static partial class DbParameterHelper
    {
        public static object SetGetObjectValue
                                    (
                                        this DbParameter target
                                        , JToken jValue
                                    )
        {
            var type = target.GetType();
            return
                SetGetObjectValue
                    (
                        target
                        , type
                        , jValue
                    );
        }
        public static object SetGetObjectValue<TDbParameter>
                            (
                                this DbParameter target
                                , JToken jValue
                            )
        {
            var type = typeof(TDbParameter);
            return
                SetGetObjectValue
                    (
                        target
                        , type
                        , jValue
                    );
        }
        public static object SetGetObjectValue
                                    (
                                        this DbParameter target
                                        , Type targetDbParameterType
                                        , JToken jValue 
                                    )
        {
            object r = null;
            if (targetDbParameterType == typeof(SqlParameter))
            {
                var parameter = (SqlParameter)target;
                r = parameter.SetGetObjectValue(jValue);
            }
            else if (targetDbParameterType == typeof(MySqlParameter))
            {
                var parameter = (MySqlParameter)target;
                r = parameter.SetGetObjectValue(jValue);
            }
            else if (targetDbParameterType == typeof(NpgsqlParameter))
            {
                var parameter = (NpgsqlParameter)target;
                r = parameter.SetGetObjectValue(jValue);
            }
            else if (targetDbParameterType == typeof(OracleParameter))
            {
                var parameter = (OracleParameter)target;
                r = parameter.SetGetObjectValue(jValue);
            }
            return r;
        }
    }
}
