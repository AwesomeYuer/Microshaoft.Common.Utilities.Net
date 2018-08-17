#if !XAMARIN && NETFRAMEWORK4_X
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using Oracle.ManagedDataAccess.Client;
    using System;
    using System.Data;
    public class OracleStoreProceduresExecutor
                    : AbstractStoreProceduresExecutor<OracleConnection, OracleCommand, OracleParameter>
    {
        protected override OracleParameter
                        OnQueryDefinitionsSetInputParameterProcess
                            (
                                OracleParameter parameter
                            )
        {
            parameter.OracleDbType = OracleDbType.Varchar2;
            return parameter;
        }
        protected override OracleParameter
                        OnQueryDefinitionsSetReturnParameterProcess
                            (
                                OracleParameter parameter
                            )
        {
            parameter.OracleDbType = OracleDbType.Int32;
            return parameter;
        }
        protected override OracleParameter
                       OnQueryDefinitionsReadOneDbParameterProcess
                           (
                               IDataReader reader
                               , OracleParameter parameter
                           )
        {
            var dbTypeName = (string)(reader["DATA_TYPE"]);
            OracleDbType dbType = (OracleDbType)Enum.Parse(typeof(OracleDbType), dbTypeName, true);
            parameter
                .OracleDbType = dbType;
            if (parameter.OracleDbType == OracleDbType.Decimal)
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
        protected override OracleParameter
                    OnExecutingSetDbParameterTypeProcess
                        (
                            OracleParameter definitionParameter
                            , OracleParameter cloneParameter
                        )
        {
            cloneParameter.OracleDbType = definitionParameter.OracleDbType;
            return cloneParameter;
        }
        protected override object
               OnExecutingSetDbParameterValueProcess
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
                    parameter.OracleDbType == OracleDbType.Boolean
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
                    parameter.OracleDbType == OracleDbType.Decimal
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
                    parameter.OracleDbType == OracleDbType.Double
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
                    parameter.OracleDbType == OracleDbType.Raw
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
                    parameter.OracleDbType == OracleDbType.Long
                    ||
                    parameter.OracleDbType == OracleDbType.Int64
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
                    parameter.OracleDbType == OracleDbType.Int32
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
                    parameter.OracleDbType == OracleDbType.Int16
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
#endif