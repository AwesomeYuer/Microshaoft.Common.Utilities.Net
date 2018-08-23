﻿#if !XAMARIN
namespace Microshaoft
{
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    public class MySqlStoreProceduresExecutor
                    : AbstractStoreProceduresExecutor<MySqlConnection, MySqlCommand, MySqlParameter>
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
                dbTypeName = "Int16";
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
            MySqlDbType dbType = MySqlDbType.Bit;
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
                    .MySqlDbType = dbType;
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
                    parameter.Scale =
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
            cloneParameter.MySqlDbType = definitionParameter.MySqlDbType;
            return cloneParameter;
        }
        protected override object
               OnExecutingSetDbParameterValueProcess
                    (
                        MySqlParameter parameter
                        , JToken jValue
                    )
        {
            object r = null;
            var jValueText = jValue.ToString();
            if
                (
                    parameter.MySqlDbType == MySqlDbType.VarChar
                    ||
                    parameter.MySqlDbType == MySqlDbType.Text
                    ||
                    parameter.MySqlDbType == MySqlDbType.VarString
                )
            {
                r = jValueText;
            }
            else if
                (
                    parameter.MySqlDbType == MySqlDbType.DateTime
                    ||
                    parameter.MySqlDbType == MySqlDbType.Date
                    ||
                    parameter.MySqlDbType == MySqlDbType.DateTime
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
                    parameter.MySqlDbType == MySqlDbType.Bit
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
                    parameter.MySqlDbType == MySqlDbType.Decimal
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
            else if
                (
                    parameter.MySqlDbType == MySqlDbType.Float
                )
            {
                var b = float
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
                    parameter.MySqlDbType == MySqlDbType.Guid
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
                    parameter.MySqlDbType == MySqlDbType.UInt16
                )
            {
                var b = ushort
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
                    parameter.MySqlDbType == MySqlDbType.UInt24
                    ||
                    parameter.MySqlDbType == MySqlDbType.UInt32
                )
            {
                var b = uint
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
                    parameter.MySqlDbType == MySqlDbType.UInt64
                )
            {
                var b = ulong
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
                   parameter.MySqlDbType == MySqlDbType.Int16
               )
            {
                var b = short
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
                    parameter.MySqlDbType == MySqlDbType.Int24
                    ||
                    parameter.MySqlDbType == MySqlDbType.Int32
               )
            {
                var b = int
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
                    parameter.MySqlDbType == MySqlDbType.Int64
               )
            {
                var b = long
                            .TryParse
                                (
                                    jValueText
                                    , out long rr
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