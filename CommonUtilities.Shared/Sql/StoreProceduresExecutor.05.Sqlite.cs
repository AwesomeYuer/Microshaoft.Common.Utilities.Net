#if !NETFRAMEWORK4_5_X && !XAMARIN
namespace Microshaoft
{
    using Microsoft.Data.Sqlite;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    public class SqliteStoreProceduresExecutor
                    : AbstractStoreProceduresExecutor<SqliteConnection, SqliteCommand, SqliteParameter>
    {
        protected override SqliteParameter
                        OnQueryDefinitionsSetInputParameterProcess
                            (
                                SqliteParameter parameter
                            )
        {
            parameter.SqliteType = SqliteType.Text;
            return parameter;
        }
        protected override SqliteParameter
                        OnQueryDefinitionsSetReturnParameterProcess
                            (
                                SqliteParameter parameter
                            )
        {
            parameter.SqliteType = SqliteType.Integer;
            return parameter;
        }
        protected override SqliteParameter
                       OnQueryDefinitionsReadOneDbParameterProcess
                           (
                               IDataReader reader
                               , SqliteParameter parameter
                           )
        {
            var originalDbTypeName = (string)(reader["DATA_TYPE"]);
            var dbTypeName = originalDbTypeName;

            SqliteType dbType = SqliteType.Integer;
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
                    .SqliteType = dbType;
            }
            return parameter;
        }
        protected override SqliteParameter
                    OnExecutingSetDbParameterTypeProcess
                        (
                            SqliteParameter definitionParameter
                            , SqliteParameter cloneParameter
                        )
        {
            cloneParameter.SqliteType = definitionParameter.SqliteType;
            return cloneParameter;
        }
        protected override object
               OnExecutingSetDbParameterValueProcess
                    (
                        SqliteParameter parameter
                        , JToken jValue
                    )
        {
            object r = null;
            var jValueText = jValue.ToString();
            if
                (
                    parameter.SqliteType == SqliteType.Text
                )
            {
                r = jValueText;
            }
            else if
                (
                    parameter.SqliteType == SqliteType.Real
                )
            {
                r = decimal.Parse(jValueText);
            }
            else if
                (
                    parameter.SqliteType == SqliteType.Real
                )
            {
                r = double.Parse(jValueText);
            }
            else if
                (
                    parameter.SqliteType == SqliteType.Integer
                )
            {
                r = int.Parse(jValueText);
            }
            return r;
        }
    }
}
#endif