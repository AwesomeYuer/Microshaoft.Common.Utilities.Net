namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;
    public static partial class DbParameterHelper
    {
        public static object SetGetValueAsObject
                                    (
                                        this DbParameter target
                                        , JToken jValue
                                    )
        {
            var type = target
                            .GetType();
            return
                SetGetValueAsObject
                    (
                        target
                        , type
                        , jValue
                    );
        }
        public static object SetGetValueAsObject<TDbParameter>
                            (
                                this DbParameter target
                                , JToken jValue
                            )
        {
            var type = typeof(TDbParameter);
            return
                SetGetValueAsObject
                    (
                        target
                        , type
                        , jValue
                    );
        }
        public static object SetGetValueAsObject
                                    (
                                        this DbParameter target
                                        , Type targetDbParameterType
                                        , JToken jValue 
                                    )
        {
            object @return = null;
            if (targetDbParameterType == typeof(SqlParameter))
            {
                var parameter = (SqlParameter) target;
                @return = parameter
                            .SetGetValueAsObject(jValue);
            }
            //else if (targetDbParameterType == typeof(MySqlParameter))
            //{
            //    var parameter = (MySqlParameter)target;
            //    r = parameter.SetGetValueAsObject(jValue);
            //}
            //else if (targetDbParameterType == typeof(NpgsqlParameter))
            //{
            //    var parameter = (NpgsqlParameter)target;
            //    r = parameter.SetGetValueAsObject(jValue);
            //}
            //else if (targetDbParameterType == typeof(OracleParameter))
            //{
            //    var parameter = (OracleParameter)target;
            //    r = parameter.SetGetValueAsObject(jValue);
            //}
            return @return;
        }
    }
}
