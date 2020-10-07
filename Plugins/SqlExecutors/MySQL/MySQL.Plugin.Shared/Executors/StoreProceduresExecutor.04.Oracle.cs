//#if !XAMARIN && NETFRAMEWORK4_X
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using Oracle.ManagedDataAccess.Client;
    using System;
    using System.Collections.Concurrent;
    using System.Data;
    public class OracleStoreProceduresExecutor
                    : AbstractStoreProceduresExecutor<OracleConnection, OracleCommand, OracleParameter>
    {
        public OracleStoreProceduresExecutor
                    (
                        ConcurrentDictionary<string, ExecutingInfo>
                            paramerersDefinitionCachingStore
                    )
                        : base
                            (
                                paramerersDefinitionCachingStore
                            )
        {

        }

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
                               , string connectionString
                           )
        {
            var dbTypeName = (string) reader["DATA_TYPE"];
            if
                (
                    Enum
                        .TryParse
                            (
                                dbTypeName
                                , true
                                , out OracleDbType dbType
                            )
                )
            {
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
            return
                parameter
                    .SetGetValueAsObject(jValue);
        }
    }
}
//#endif