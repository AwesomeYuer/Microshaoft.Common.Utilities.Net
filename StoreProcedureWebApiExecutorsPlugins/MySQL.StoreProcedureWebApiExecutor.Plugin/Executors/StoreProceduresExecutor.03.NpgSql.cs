#if !XAMARIN
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using Npgsql;
    using NpgsqlTypes;
    using System;
    using System.Collections.Generic;
    using System.Data;
    public class NpgSqlStoreProceduresExecutor
                    : AbstractStoreProceduresExecutor
                        <
                            NpgsqlConnection
                            , NpgsqlCommand
                            , NpgsqlParameter
                        >
    {
        private string _parametersQueryCommandText = $@"
                    SELECT
                        * 
                    FROM
                        information_schema.parameters a 
                    WHERE
                        a.SPECIFIC_NAME = @ProcedureName
                    order by
                        a.SPECIFIC_NAME
                    limit 1
                    ";
        public override string ParametersQueryCommandText
        {
            get => _parametersQueryCommandText;
        }


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
            NpgsqlDbType dbType = (NpgsqlDbType) Enum.Parse(typeof(NpgsqlDbType), dbTypeName, true);
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
            return
                parameter
                    .SetGetValueAsObject
                        (
                            jValue
                        );
        }
        public override IEnumerable<NpgsqlParameter> GetDefinitionParameters
            (
                string connectionString
                , string storeProcedureName
                , bool includeReturnValueParameter = false
            )
        {

            return
                SqlHelper
                        .GetStoreProcedureDefinitionParameters
                                <NpgsqlConnection, NpgsqlCommand, NpgsqlParameter>
                                    (
                                        connectionString
                                        , storeProcedureName
                                        , OnQueryDefinitionsSetInputParameterProcess
                                        , OnQueryDefinitionsSetReturnParameterProcess
                                        , OnQueryDefinitionsReadOneDbParameterProcess
                                        , ParametersQueryCommandText
                                        , includeReturnValueParameter
                                    );
        }
    }
}
#endif