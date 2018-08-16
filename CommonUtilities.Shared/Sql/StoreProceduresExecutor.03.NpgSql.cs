#if !XAMARIN
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using Npgsql;
    using NpgsqlTypes;
    using System;
    using System.Data;
    public class NpgSqlStoreProceduresExecutor
                    : AbstractStoreProceduresExecutor<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>
    {
        protected override NpgsqlParameter
                        OnQueryDefinitionsSetInputParameterProcess
                            (
                                NpgsqlParameter parameter
                            )
        {
            parameter.NpgsqlDbType = NpgsqlDbType.Varchar;
            return parameter;
        }
        protected override NpgsqlParameter
                        OnQueryDefinitionsSetReturnParameterProcess
                            (
                                NpgsqlParameter parameter
                            )
        {
            parameter.NpgsqlDbType = NpgsqlDbType.Integer;
            return parameter;
        }
        protected override NpgsqlParameter
                       OnQueryDefinitionsReadOneDbParameterProcess
                           (
                               IDataReader reader
                               , NpgsqlParameter parameter
                           )
        {
            var dbTypeName = (string)(reader["DATA_TYPE"]);
            NpgsqlDbType dbType = (NpgsqlDbType)Enum.Parse(typeof(NpgsqlDbType), dbTypeName, true);
            parameter
                .NpgsqlDbType = dbType;
            if ((parameter.NpgsqlDbType == NpgsqlDbType.Numeric))
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
        protected override NpgsqlParameter
                    OnExecutingSetDbParameterTypeProcess
                        (
                            NpgsqlParameter definitionParameter
                            , NpgsqlParameter cloneParameter
                        )
        {
            cloneParameter.NpgsqlDbType = definitionParameter.NpgsqlDbType;
            return cloneParameter;
        }
        protected override object
               OnExecutingSetDbParameterValueProcess
                    (
                        NpgsqlParameter parameter
                        , JToken jValue
                    )
        {
            object r = null;
            var jValueText = jValue.ToString();
            if
                (
                    parameter.NpgsqlDbType == NpgsqlDbType.Varchar
                    ||
                    parameter.NpgsqlDbType == NpgsqlDbType.Text
                    ||
                    parameter.NpgsqlDbType == NpgsqlDbType.Char
                )
            {
                r = jValueText;
            }
            else if
                (
                    parameter.NpgsqlDbType == NpgsqlDbType.Date
                    ||
                    parameter.NpgsqlDbType == NpgsqlDbType.Time
                )
            {
                r = DateTime.Parse(jValueText);
            }
            else if
                (
                    parameter.NpgsqlDbType == NpgsqlDbType.Bit
                )
            {
                r = bool.Parse(jValueText);
            }
            else if
                (
                    parameter.NpgsqlDbType == NpgsqlDbType.Double
                )
            {
                r = Double.Parse(jValueText);
            }
            else if
                (
                    parameter.NpgsqlDbType == NpgsqlDbType.Real
                )
            {
                r = Double.Parse(jValueText);
            }
            else if
                (
                    parameter.NpgsqlDbType == NpgsqlDbType.Uuid
                )
            {
                r = Guid.Parse(jValueText);
            }
            else if
               (
                    parameter.NpgsqlDbType == NpgsqlDbType.Bigint
               )
            {
                r = long.Parse(jValueText);
            }
            return r;
        }
    }
}
#endif