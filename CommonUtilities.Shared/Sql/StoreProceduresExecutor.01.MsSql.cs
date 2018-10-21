namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    public class MsSqlStoreProceduresExecutor
                    : AbstractStoreProceduresExecutor
                            <SqlConnection, SqlCommand, SqlParameter>
    {
        private IDictionary<string, Type> _dbTypesMapper
                    = new Dictionary<string, Type>
                            (StringComparer.OrdinalIgnoreCase)
        {
              { "image"                 , typeof(byte[])            }
            , { "text"                  , typeof(string)            }
            , { "uniqueidentifier"      , typeof(Guid)              }
            , { "date"                  , typeof(DateTime)          }
            , { "time"                  , typeof(DateTime)          }
            , { "datetime2"             , typeof(DateTime)          }
            , { "datetimeoffset"        , typeof(DateTime)          }
            , { "tinyint"               , typeof(short)             }
            , { "smallint"              , typeof(int)               }
            , { "int"                   , typeof(int)               }
            , { "smalldatetime"         , typeof(DateTime)          }
            , { "real"                  , typeof(double)            }
            , { "money"                 , typeof(decimal)           }
            , { "datetime"              , typeof(DateTime)          }
            , { "float"                 , typeof(float)             }
            , { "sql_variant"           , typeof(byte[])            }
            , { "ntext"                 , typeof(string)            }
            , { "bit"                   , typeof(bool)              }
            , { "decimal"               , typeof(decimal)           }
            , { "numeric"               , typeof(decimal)           }
            , { "smallmoney"            , typeof(decimal)           }
            , { "bigint"                , typeof(long)              }
            , { "hierarchyid"           , typeof(long)              }
            , { "geometry"              , typeof(string)            }
            , { "geography"             , typeof(string)            }
            , { "varbinary"             , typeof(byte[])            }
            , { "varchar"               , typeof(string)            }
            , { "binary"                , typeof(byte[])            }
            , { "char"                  , typeof(string)            }
            , { "timestamp"             , typeof(long)              }
            , { "nvarchar"              , typeof(string)            }
            , { "nchar"                 , typeof(string)            }
            , { "xml"                   , typeof(string)            }
            , { "sysname"               , typeof(string)            }
        };
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
                               , string connectionString
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
            else if (string.Compare(dbTypeName, "table type", true) == 0)
            {
                dbTypeName = "Structured";
            }
            SqlDbType dbType;
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
            else if (parameter.SqlDbType == SqlDbType.Udt)
            {
                //, @geometry geometry = null
                //, @geography geography = null
                parameter.UdtTypeName = originalDbTypeName;
            }
            else if (parameter.SqlDbType == SqlDbType.Structured)
            {
                var o = reader["USER_DEFINED_TYPE_NAME"];
                if (o != DBNull.Value)
                {
                    parameter.TypeName = (string)o;
                }
                var dataTable = CreateUserDefinedTableTypeEmptyDataTable
                                    (
                                        connectionString
                                        , parameter.TypeName
                                    );
                parameter.Value = dataTable;
            }
            return parameter;
        }
        public Type GetTypeBySqlDbTypeName(string sqlDbTypeName)
        {
            Type r = null;
            _dbTypesMapper
                    .TryGetValue
                        (
                            sqlDbTypeName
                            , out r
                        );
            return r;
        }
        public DataTable
                        CreateUserDefinedTableTypeEmptyDataTable
                            (
                                string connectionString
                                , string userDefinedTableTypeName
                            )
        {
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection();
                connection.ConnectionString = connectionString;
                var dataSource = connection.DataSource;
                var dataBase = connection.Database;
                string procedureSchema = string.Empty;
                string parameterName = string.Empty;
                var commandText = @"
select
	a.name			as ColumnName
	, b.name		as ColumnTypeName
	, a.*
from
	(
		SELECT
			*
		FROM
			sys.columns a
		WHERE
			object_id
			IN
				(
					SELECT
						aa.type_table_object_id
					FROM
						sys.table_types aa
					WHERE
						aa.name = @userDefinedTableTypeName
				)
	) a
		inner join
			sys.types b
				on
					a.system_type_id = b.system_type_id
					and
					a.user_type_id = b.user_type_id
                ";
                //MySQL 不支持 using command
                using
                    (
                        SqlCommand command = new SqlCommand()
                        {
                            CommandText = commandText
                            ,
                            CommandType = CommandType.Text
                            ,
                            Connection = connection
                        }
                    )
                {
                    //command.CommandType = CommandType.StoredProcedure;
                    SqlParameter dbParameterUserDefinedTableTypeName = new SqlParameter();
                    dbParameterUserDefinedTableTypeName
                            .ParameterName = "@userDefinedTableTypeName";
                    dbParameterUserDefinedTableTypeName
                            .Direction = ParameterDirection.Input;
                    dbParameterUserDefinedTableTypeName
                            .Size = 128;
                    dbParameterUserDefinedTableTypeName
                            .Value =
                                    (
                                        userDefinedTableTypeName != null
                                        ?
                                        (object)userDefinedTableTypeName
                                        :
                                        DBNull.Value
                                    );
                    dbParameterUserDefinedTableTypeName
                            .SqlDbType = SqlDbType.VarChar;
                    command
                        .Parameters
                        .Add
                            (dbParameterUserDefinedTableTypeName);
                    connection.Open();
                    var dataReader
                            = command
                                    .ExecuteReader
                                        (
                                            CommandBehavior
                                                .CloseConnection
                                        );
                    var dataTable = new DataTable();
                    dataReader
                        .ExecuteRead
                            (
                                (x, reader) =>
                                {
                                    var columnName = (string)(reader["ColumnName"]);
                                    var columnTypeName = (string)(reader["columnTypeName"]);
                                    var columnType = GetTypeBySqlDbTypeName(columnTypeName);
                                    dataTable
                                        .Columns
                                        .Add
                                            (
                                                columnName
                                                , columnType
                                            );
                                    return true;
                                }
                            )
                            .ToArray();
                    return
                        dataTable;
                }
            }
            finally
            {
                //connection.Close();
                //connection = null;
            }
        }
        protected override SqlParameter
                    OnExecutingSetDbParameterTypeProcess
                        (
                            SqlParameter definitionParameter
                            , SqlParameter cloneParameter
                        )
        {
            cloneParameter
                    .SqlDbType = definitionParameter.SqlDbType;
            return
                cloneParameter;
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
                    parameter.SqlDbType == SqlDbType.DateTimeOffset
                )
            {
                var b = DateTimeOffset
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
                    parameter.SqlDbType == SqlDbType.Bit
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
                    parameter.SqlDbType == SqlDbType.Decimal
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
                    parameter.SqlDbType == SqlDbType.Float
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
                    parameter.SqlDbType == SqlDbType.Real
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
                    parameter.SqlDbType == SqlDbType.UniqueIdentifier
                )
            {
                var b = Guid
                            .TryParse
                                (
                                    jValueText
                                    , out Guid rr
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
                    parameter.SqlDbType == SqlDbType.Int
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
                    parameter.SqlDbType == SqlDbType.SmallInt
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
                    parameter.SqlDbType == SqlDbType.TinyInt
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
                    parameter.SqlDbType == SqlDbType.Structured
                )
            {
                var parameterValue = parameter.Value;
                if
                    (
                        parameterValue != DBNull.Value
                        &&
                        parameterValue != null
                    )
                {
                    var dataTable = parameterValue as DataTable;
                    if (dataTable != null)
                    {
                        var jArray = (JArray)jValue;
                        var columns = dataTable.Columns;
                        var rows = dataTable.Rows;
                        foreach (var entry in jArray)
                        {
                            var row = dataTable.NewRow();
                            foreach (DataColumn column in columns)
                            {
                                var columnName = column.ColumnName;
                                var jToken = entry[columnName];
                                var cell = row[columnName];
                                // with ""
                                if (column.DataType == typeof(string))
                                {
                                    var jValueString = jToken.Value<string>();
                                    cell = jToken.Value<string>();
                                }
                                else if (column.DataType == typeof(Guid))
                                {
                                    var jValueString = jToken.Value<string>();
                                    cell = Guid.Parse(jValueString);
                                }
                                else if (column.DataType == typeof(DateTime))
                                {
                                    var jValueString = jToken.Value<string>();
                                    cell = DateTime.Parse(jValueString);
                                }
                                //===============================================
                                // without ""
                                else if (column.DataType == typeof(bool))
                                {
                                    cell = jToken.Value<bool>();
                                }
                                else if (column.DataType == typeof(short))
                                {
                                    cell = jToken.Value<short>();
                                }
                                else if (column.DataType == typeof(ushort))
                                {
                                    cell = jToken.Value<ushort>();
                                }
                                else if (column.DataType == typeof(int))
                                {
                                    cell = jToken.Value<int>();
                                }
                                else if (column.DataType == typeof(uint))
                                {
                                    cell = jToken.Value<uint>();
                                }
                                else if (column.DataType == typeof(long))
                                {
                                    cell = jToken.Value<long>();
                                }
                                else if (column.DataType == typeof(ulong))
                                {
                                    cell = jToken.Value<ulong>();
                                }
                                else if (column.DataType == typeof(double))
                                {
                                    cell = jToken.Value<double>();
                                }
                                else if (column.DataType == typeof(float))
                                {
                                    cell = jToken.Value<float>();
                                }
                                else if (column.DataType == typeof(decimal))
                                {
                                    cell = jToken.Value<decimal>();
                                }
                                else
                                {
                                    cell = null;
                                }
                            }
                            rows.Add(row);
                        }
                        r = dataTable;
                    }
                }
            }
            return r;
        }
    }
}