#if !XAMARIN
namespace Microshaoft
{
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    public class MySqlStoreProceduresExecutor
                    : AbstractStoreProceduresExecutor<MySqlConnection, MySqlCommand, MySqlParameter>
    {
        protected override MySqlParameter
                        OnQueryDefinitionsSetInputParameterProcess
                            (
                                MySqlParameter parameter
                            )
        {
            parameter.MySqlDbType = MySqlDbType.VarChar;
            return parameter;
        }
        protected override MySqlParameter
                        OnQueryDefinitionsSetReturnParameterProcess
                            (
                                MySqlParameter parameter
                            )
        {
            parameter.MySqlDbType = MySqlDbType.Int32;
            return parameter;
        }
        protected override MySqlParameter
                       OnQueryDefinitionsReadOneDbParameterProcess
                           (
                               IDataReader reader
                               , MySqlParameter parameter
                           )
        {
            var dbTypeName = (string)(reader["DATA_TYPE"]);
            MySqlDbType dbType = (MySqlDbType)Enum.Parse(typeof(MySqlDbType), dbTypeName, true);
            parameter
                .MySqlDbType = dbType;
            if ((parameter.MySqlDbType == MySqlDbType.Decimal))
            {
                parameter.Scale =
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
            return parameter;
        }
        protected override MySqlParameter
                    OnExecutingSetDbParameterTypeProcess
                        (
                            MySqlParameter definitionParameter
                            , MySqlParameter cloneParameter
                        )
        {
            cloneParameter.MySqlDbType = definitionParameter.MySqlDbType;
            return cloneParameter;
        }
        protected override object
               OnExecutingSetDbParameterValueProcess
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
                    ||
                    parameter.MySqlDbType == MySqlDbType.Text
                    ||
                    parameter.MySqlDbType == MySqlDbType.VarString
                )
            {
                r = jValueText;
            }
            else if
                (
                    parameter.MySqlDbType == MySqlDbType.DateTime
                    ||
                    parameter.MySqlDbType == MySqlDbType.Date
                    ||
                    parameter.MySqlDbType == MySqlDbType.DateTime
                )
            {
                r = DateTime.Parse(jValueText);
            }
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


    }
}

#endif