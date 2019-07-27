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
        // PostgreSQL can't full support information_schema.parameters 
        /*
         * https://www.postgresql.org/docs/12/infoschema-parameters.html
         * character_maximum_length	cardinal_number	Always null, since this information is not applied to parameter data types in PostgreSQL
         * character_octet_length	cardinal_number	Always null, since this information is not applied to parameter data types in PostgreSQL
         */
        private string _parametersQueryCommandText = $@"
select
	*
from
	information_schema.parameters a
where
	a.SPECIFIC_NAME =
        (
            SELECT
                aa.SPECIFIC_NAME
            FROM
                information_schema.parameters aa 
            WHERE
                aa.SPECIFIC_NAME like @ProcedureName || '%'
            order by
                aa.SPECIFIC_NAME
            limit 1
	    )
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
            var dbTypeName = (string) reader["DATA_TYPE"];
            if
                (
                    Enum
                        .TryParse
                            (
                                dbTypeName
                                , true
                                , out NpgsqlDbType dbType
                            )
                )
            {
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
                GetStoreProcedureDefinitionParameters
                        (
                            connectionString
                            , storeProcedureName
                            , _parametersQueryCommandText
                            , includeReturnValueParameter
                        );
        }
    }
}
#endif