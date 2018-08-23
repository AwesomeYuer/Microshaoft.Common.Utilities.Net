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
                               , string connectionString
                           )
        {
            var dbTypeName = (string)(reader["DATA_TYPE"]);
            NpgsqlDbType dbType = (NpgsqlDbType)Enum.Parse(typeof(NpgsqlDbType), dbTypeName, true);
            parameter
                .NpgsqlDbType = dbType;
            if ((parameter.NpgsqlDbType == NpgsqlDbType.Numeric))
            {
                var o = reader["NUMERIC_SCALE"];
                if (o != DBNull.Value)
                {
                    parameter
                        .Scale =
                            (
                                (byte)
                                    (
                                        (
                                            (short)
                                                (
                                                    (int)o
                                                )
                                        )
                                    //& 255
                                    )
                            );
                }
                o = reader["NUMERIC_PRECISION"];
                if (o != DBNull.Value)
                {
                    parameter.Precision = ((byte)o);
                }
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
                var b = DateTime
                            .TryParse
                                (
                                    jValueText
                                    , out var rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.NpgsqlDbType == NpgsqlDbType.Bit
                )
            {
                var b = bool
                            .TryParse
                                (
                                    jValueText
                                    , out var rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.NpgsqlDbType == NpgsqlDbType.Double
                    ||
                    parameter.NpgsqlDbType == NpgsqlDbType.Real
                )
            {
                var b = double
                            .TryParse
                                (
                                    jValueText
                                    , out var rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.NpgsqlDbType == NpgsqlDbType.Uuid
                )
            {
                var b = Guid
                            .TryParse
                                (
                                    jValueText
                                    , out var rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
               (
                    parameter.NpgsqlDbType == NpgsqlDbType.Bigint
               )
            {
                var b = long
                            .TryParse
                                (
                                    jValueText
                                    , out var rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
               (
                    parameter.NpgsqlDbType == NpgsqlDbType.Numeric
               )
            {
                 var b = decimal
                            .TryParse
                                (
                                    jValueText
                                    , out var rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            return r;
        }
    }
}
#endif