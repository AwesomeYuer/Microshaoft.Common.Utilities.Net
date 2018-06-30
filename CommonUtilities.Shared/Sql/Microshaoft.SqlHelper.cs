namespace Microshaoft
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.Data.SqlClient;
    using System.Linq;
#if !NETFRAMEWORK4_X
    using Microshaoft.Data.OleDb;
#endif
    public static class SqlHelper
    {
        public class SqlParameterDefinition
        {
            public string ParameterName;
            [JsonProperty("SqlDbType")]
            public string SqlDbTypeName;
            public string Size;
            public ParameterDirection Direction;
            public byte Precision;
        }

        public static SqlParameter[] GenerateExecuteSqlParameters(SqlConnection connection, string storeProcedureName, JObject actualParameters)
        {
            var sqlParameters = GetCachedStoreProcedureParameters(connection, storeProcedureName, true);
            var actualSqlParameters = new SqlParameter[sqlParameters.Length];
            var i = 0;
            foreach (var sqlParameter in sqlParameters)
            {
                var parameterName = sqlParameter.ParameterName;
                var hasActualParameter = actualParameters[parameterName] != null;
                var actualSqlParameter = sqlParameter.Clone(!hasActualParameter);
                if (hasActualParameter)
                {
                    actualSqlParameter.Value = (object)actualParameters[parameterName];
                }
                actualSqlParameters[i] = actualSqlParameter;
                i++;
            }

            return actualSqlParameters;
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

        public static SqlParameter Clone(this SqlParameter target, bool includeValue = false)
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


        public static ConcurrentDictionary<string, SqlParameter[]> _dictionary = new ConcurrentDictionary<string, SqlParameter[]>();
        public static SqlParameter[] GetCachedStoreProcedureParameters
                                        (
                                            SqlConnection connection
                                            , string p_procedure_name
                                            , bool includeReturnValueParameter = false
                                        )
        {
            

            var key = $"{connection.DataSource}-{connection.Database}-{p_procedure_name}".ToUpper();
            var result = _dictionary
                                .GetOrAdd
                                        (
                                            key
                                            , (x) =>
                                            {
                                                var sqlParameters = GetStoreProcedureParameters
                                                    (
                                                        connection
                                                        , p_procedure_name
                                                        , includeReturnValueParameter
                                                    );
                                                var r = sqlParameters
                                                                .ToArray();
                                                return r;
                                            }
                                        );
            return result;
        }



        public static IEnumerable<SqlParameter> GetStoreProcedureParameters
                                        (
                                            SqlConnection connection
                                            , string p_procedure_name
                                            , bool includeReturnValueParameter = false
                                        )
        {

            int p_group_number = 0;
            string p_procedure_schema = string.Empty;
            string p_parameter_name = string.Empty;


            SqlCommand command = new SqlCommand("sp_procedure_params_rowset", connection);
            command.CommandType = CommandType.StoredProcedure;
            
            SqlParameter sqlParameterProcedure_Name = command.Parameters.Add("@procedure_name", SqlDbType.NVarChar, 128);
            sqlParameterProcedure_Name.Value = (p_procedure_name != null ? (object)p_procedure_name : DBNull.Value);
            SqlParameter sqlParameterGroup_Number = command.Parameters.Add("@group_number", SqlDbType.Int);
            sqlParameterGroup_Number.Value = p_group_number;
            SqlParameter sqlParameterProcedure_Schema = command.Parameters.Add("@procedure_schema", SqlDbType.NVarChar, 128);
            sqlParameterProcedure_Schema.Value = (p_procedure_schema != null ? (object)p_procedure_schema : DBNull.Value);
            SqlParameter sqlParameterParameter_Name = command.Parameters.Add("@parameter_name", SqlDbType.NVarChar, 128);
            sqlParameterParameter_Name.Value = (p_parameter_name != null ? (object)p_parameter_name : DBNull.Value);
            SqlParameter sqlParameterReturn = command.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
            sqlParameterReturn.Direction = ParameterDirection.ReturnValue;
            connection.Open();

            var sqlDataReader = command.ExecuteReader(CommandBehavior.CloseConnection);

            var sqlParameters = sqlDataReader.ExecuteRead<SqlParameter>
                    (
                        (x, reader) =>
                        {
                            var sqlParameter = new SqlParameter();
                            sqlParameter
                                .ParameterName = (string)(reader["PARAMETER_NAME"]);
                            sqlParameter
                                .SqlDbType = GetSqlDbType
                                                (
                                                    (short)(reader["DATA_TYPE"])

                                                    , (string)(reader["TYPE_NAME"])
                                                );
                            if (reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value)
                            {
                                sqlParameter
                                    .Size = reader.GetInt32(reader.GetOrdinal("CHARACTER_MAXIMUM_LENGTH"));
                            }
                            
                            sqlParameter
                                    .Direction = GetParameterDirection
                                                    (
                                                        reader.GetInt16(reader.GetOrdinal("PARAMETER_TYPE"))
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
        private static SqlDbType GetSqlDbType(short parameterType, string typeName)
        {
            SqlDbType cmdType;
            OleDbType oleDbType;
            cmdType = SqlDbType.Variant;
            oleDbType = (OleDbType)(parameterType);

            switch (oleDbType)
            {
                case OleDbType.SmallInt:
                    cmdType = SqlDbType.SmallInt;
                    break;
                case OleDbType.Integer:
                    cmdType = SqlDbType.Int;
                    break;
                case OleDbType.Single:
                    cmdType = SqlDbType.Real;
                    break;
                case OleDbType.Double:
                    cmdType = SqlDbType.Float;
                    break;
                case OleDbType.Currency:
                    cmdType = (typeName == "money") ? SqlDbType.Money : SqlDbType.SmallMoney;
                    break;
                case OleDbType.Date:
                    cmdType = (typeName == "datetime") ? SqlDbType.DateTime : SqlDbType.SmallDateTime;
                    break;
                case OleDbType.BSTR:
                    cmdType = (typeName == "nchar") ? SqlDbType.NChar : SqlDbType.NVarChar;
                    break;
                case OleDbType.Boolean:
                    cmdType = SqlDbType.Bit;
                    break;
                case OleDbType.Variant:
                    cmdType = SqlDbType.Variant;
                    break;
                case OleDbType.Decimal:
                    cmdType = SqlDbType.Decimal;
                    break;
                case OleDbType.TinyInt:
                    cmdType = SqlDbType.TinyInt;
                    break;
                case OleDbType.UnsignedTinyInt:
                    cmdType = SqlDbType.TinyInt;
                    break;
                case OleDbType.UnsignedSmallInt:
                    cmdType = SqlDbType.SmallInt;
                    break;
                case OleDbType.BigInt:
                    cmdType = SqlDbType.BigInt;
                    break;
                case OleDbType.Filetime:
                    cmdType = (typeName == "datetime") ? SqlDbType.DateTime : SqlDbType.SmallDateTime;
                    break;
                case OleDbType.Guid:
                    cmdType = SqlDbType.UniqueIdentifier;
                    break;
                case OleDbType.Binary:
                    cmdType = (typeName == "binary") ? SqlDbType.Binary : SqlDbType.VarBinary;
                    break;
                case OleDbType.Char:
                    cmdType = (typeName == "char") ? SqlDbType.Char : SqlDbType.VarChar;
                    break;
                case OleDbType.WChar:
                    cmdType = (typeName == "nchar") ? SqlDbType.NChar : SqlDbType.NVarChar;
                    break;
                case OleDbType.Numeric:
                    cmdType = SqlDbType.Decimal;
                    break;
                case OleDbType.DBDate:
                    cmdType = (typeName == "datetime") ? SqlDbType.DateTime : SqlDbType.SmallDateTime;
                    break;
                case OleDbType.DBTime:
                    cmdType = (typeName == "datetime") ? SqlDbType.DateTime : SqlDbType.SmallDateTime;
                    break;
                case OleDbType.DBTimeStamp:
                    cmdType = (typeName == "datetime") ? SqlDbType.DateTime : SqlDbType.SmallDateTime;
                    break;
                case OleDbType.VarChar:
                    cmdType = (typeName == "char") ? SqlDbType.Char : SqlDbType.VarChar;
                    break;
                case OleDbType.LongVarChar:
                    cmdType = SqlDbType.Text;
                    break;
                case OleDbType.VarWChar:
                    cmdType = (typeName == "nchar") ? SqlDbType.NChar : SqlDbType.NVarChar;
                    break;
                case OleDbType.LongVarWChar:
                    cmdType = SqlDbType.NText;
                    break;
                case OleDbType.VarBinary:
                    cmdType = (typeName == "binary") ? SqlDbType.Binary : SqlDbType.VarBinary;
                    break;
                case OleDbType.LongVarBinary:
                    cmdType = SqlDbType.Image;
                    break;
            }
            return cmdType;
        }

    }


}
