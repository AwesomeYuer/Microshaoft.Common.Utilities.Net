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
            if ((parameter.OracleDbType == OracleDbType.Decimal))
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
    }
}
#endif