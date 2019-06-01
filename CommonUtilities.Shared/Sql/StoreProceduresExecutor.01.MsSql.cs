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
                    parameter.Precision = ((byte) o);
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
            _dbTypesMapper
                    .TryGetValue
                        (
                            sqlDbTypeName
                            , out var r
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
			sys.columns aa
		WHERE
			aa.object_id
			IN
				(
					SELECT
						aaa.type_table_object_id
					FROM
						sys.table_types aaa
					WHERE
						aaa.name = @userDefinedTableTypeName
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
            return
                parameter
                    .SetGetValueAsObject
                        (
                            jValue
                        );
        }
    }
}
