namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    public class MsSqlStoreProceduresExecutor
                    : AbstractStoreProceduresExecutor<SqlConnection, SqlCommand, SqlParameter>
    {
        protected override SqlParameter
                        OnQueryDefinitionsSetInputParameterProcess
                            (
                                SqlParameter parameter
                            )
        {
            parameter.SqlDbType = SqlDbType.NVarChar;
            return parameter;
        }
        protected override SqlParameter
                        OnQueryDefinitionsSetReturnParameterProcess
                            (
                                SqlParameter parameter
                            )
        {
            parameter.SqlDbType = SqlDbType.Int;
            return parameter;
        }
        protected override SqlParameter
                       OnQueryDefinitionsReadOneDbParameterProcess
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
            var r = Enum
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
            if (parameter.SqlDbType == SqlDbType.Decimal)
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
                                                    (int) o
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
            else if (parameter.SqlDbType == SqlDbType.Udt)
            {
                //, @geometry geometry = null
                //, @geography geography = null
                parameter.UdtTypeName = originalDbTypeName;
            }
            return parameter;
        }
        protected override SqlParameter
                    OnExecutingSetDbParameterTypeProcess
                        (
                            SqlParameter definitionParameter
                            , SqlParameter cloneParameter
                        )
        {
            cloneParameter.SqlDbType = definitionParameter.SqlDbType;
            return cloneParameter;
        }
        protected override object
               OnExecutingSetDbParameterValueProcess
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
                DateTime rr;
                var b = DateTime
                            .TryParse
                                (
                                    jValueText
                                    , out rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.DateTimeOffset
                )
            {
                DateTimeOffset rr;
                var b = DateTimeOffset
                            .TryParse
                                (
                                    jValueText
                                    , out rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Bit
                )
            {
                bool rr;
                var b = bool
                            .TryParse
                                (
                                    jValueText
                                    , out rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Decimal
                )
            {
                decimal rr;
                var b = decimal
                            .TryParse
                                (
                                    jValueText
                                    , out rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Float
                )
            {
                float rr;
                var b = float
                            .TryParse
                                (
                                    jValueText
                                    , out rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Real
                )
            {
                double rr;
                var b = double
                            .TryParse
                                (
                                    jValueText
                                    , out rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.UniqueIdentifier
                )
            {
                Guid rr;
                var b = Guid
                            .TryParse
                                (
                                    jValueText
                                    , out rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.BigInt
                )
            {
                long rr;
                var b = long
                        .TryParse
                            (
                                jValueText
                                , out rr
                            );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.Int
                )
            {
                int rr;
                var b = int
                            .TryParse
                                (
                                    jValueText
                                    , out rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.SmallInt
                )
            {
                short rr;
                var b = short
                            .TryParse
                                (
                                    jValueText
                                    , out rr
                                );
                if (b)
                {
                    r = rr;
                }
            }
            else if
                (
                    parameter.SqlDbType == SqlDbType.TinyInt
                )
            {
                short rr;
                var b = short
                            .TryParse
                                (
                                    jValueText
                                    , out rr
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