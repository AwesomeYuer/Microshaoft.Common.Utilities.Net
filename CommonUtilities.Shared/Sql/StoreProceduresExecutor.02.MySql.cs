namespace Microshaoft
{
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    public class MySqlStoreProceduresExecutor
                    : AbstractStoreProceduresExecutor
                            <MySqlConnection, MySqlCommand, MySqlParameter>
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
                               , string connectionString
                           )
        {
            var originalDbTypeName = (string)(reader["DATA_TYPE"]);
            var dbTypeName = originalDbTypeName;
            //bit
            //tinyint
            //smallint
            //mediumint
            //int
            //bigint
            //float
            //double
            //decimal
            //char
            //varchar
            //tinytext
            //tinytext
            //mediumtext
            //longtext
            //tinyblob
            //tinyblob
            //mediumblob
            //longblob
            //date
            //datetime
            //timestamp
            //time
            //year
            if (string.Compare(dbTypeName, "bool", true) == 0)
            {
                dbTypeName = "Bit";
            }
            else if (string.Compare(dbTypeName, "tinyint", true) == 0)
            {
                dbTypeName = "Byte";
            }
            else if (string.Compare(dbTypeName, "smallint", true) == 0)
            {
                dbTypeName = "Int16";
            }
            else if (string.Compare(dbTypeName, "mediumint", true) == 0)
            {
                dbTypeName = "Int24";
            }
            else if (string.Compare(dbTypeName, "int", true) == 0)
            {
                dbTypeName = "Int32";
            }
            else if (string.Compare(dbTypeName, "bigint", true) == 0)
            {
                dbTypeName = "Int64";
            }
            
            var r = Enum
                        .TryParse
                            (
                                dbTypeName
                                , true
                                , out MySqlDbType mySqlDbType
                            );
            if (r)
            {
                parameter
                    .MySqlDbType = mySqlDbType;
            }
            if
                (
                    parameter.MySqlDbType == MySqlDbType.Decimal
                    ||
                    parameter.MySqlDbType == MySqlDbType.NewDecimal
                )
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
                                                    (long) o
                                                )
                                        )
                                    //& 255
                                    )
                            );
                }
                o = reader["NUMERIC_PRECISION"];
                if (o != DBNull.Value)
                {
                    parameter.Precision = ((byte)((uint)o));
                }
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
            cloneParameter
                    .MySqlDbType = definitionParameter.MySqlDbType;
            return
                cloneParameter;
        }
        protected override object
               OnExecutingSetDbParameterValueProcess
                    (
                        MySqlParameter parameter
                        , JToken jValue
                    )
        {
            return
                parameter
                    .SetGetObjectValue
                        (
                            jValue
                        );

        }
    }
}