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

        private readonly string _parametersQueryCommandText =
@"
(
    SELECT

         DB_NAME()                                  AS SPECIFIC_CATALOG
        , SCHEMA_NAME(o.schema_id)                  AS SPECIFIC_SCHEMA
        , o.name                                    AS SPECIFIC_NAME
        , c.parameter_id                            AS ORDINAL_POSITION
        ,
            convert
                (
                    nvarchar(10)
                    ,
                        CASE
                            WHEN
                                c.parameter_id = 0
                                    THEN
                                        'OUT'
                            WHEN
                                c.is_output = 1
                                    THEN
                                        'INOUT'
                            ELSE
                                'IN'
                        END
                )                                   AS PARAMETER_MODE
        ,
            convert
                (
                    nvarchar(10)
                    ,
                        CASE
                            WHEN
                                c.parameter_id = 0
                                    THEN
                                        'YES'
                            ELSE
                                'NO'
                        END
                )                                           AS IS_RESULT
        , convert(nvarchar(10), 'NO')                       AS AS_LOCATOR
        , c.name                                            AS PARAMETER_NAME
        , ISNULL(TYPE_NAME(c.system_type_id), u.name)       AS DATA_TYPE
        , COLUMNPROPERTY
            (c.object_id, c.name, 'charmaxlen')             AS CHARACTER_MAXIMUM_LENGTH
        , COLUMNPROPERTY
            (c.object_id, c.name, 'octetmaxlen')            AS CHARACTER_OCTET_LENGTH
        , convert(sysname, null)                            AS COLLATION_CATALOG
        , convert(sysname, null) collate catalog_default    AS COLLATION_SCHEMA
        ,
            convert
                (
                    sysname
                    ,
                        CASE
                            WHEN
                                c.system_type_id IN(35, 99, 167, 175, 231, 239)
                                    THEN--[n]char /[n]varchar /[n]text
                                        SERVERPROPERTY('collation')
                        END
                )                                              AS COLLATION_NAME
        , convert(sysname, null)                               AS CHARACTER_SET_CATALOG
        , convert(sysname, null) collate catalog_default       AS CHARACTER_SET_SCHEMA
        ,
            convert
                (
                    sysname
                    ,
                        CASE
                            WHEN
                                c.system_type_id IN(35, 167, 175)
                                    THEN
                                        SERVERPROPERTY('sqlcharsetname')-- char / varchar / text
                            WHEN
                                c.system_type_id IN(99, 231, 239)
                                    THEN
                                        N'UNICODE'-- nchar / nvarchar / ntext
                        END
                )                                               AS CHARACTER_SET_NAME
        ,
            convert
                (
                    tinyint
                    ,
                        CASE-- int / decimal / numeric / real / float / money
                            WHEN
                                c.system_type_id IN(48, 52, 56, 59, 60, 62, 106, 108, 122, 127)
                                    THEN
                                        c.precision
                        END
                )                                               AS NUMERIC_PRECISION
        ,
            convert
                (
                    smallint
                        ,
                            CASE-- int / money / decimal / numeric
                                WHEN
                                    c.system_type_id IN(48, 52, 56, 60, 106, 108, 122, 127)
                                        THEN
                                            10
                                WHEN
                                    c.system_type_id IN(59, 62)
                                        THEN
                                            2
                            END
                )                                               AS NUMERIC_PRECISION_RADIX
        , --real / float
            convert
                (
                    int
                    ,
                        CASE-- datetime / smalldatetime
                            WHEN
                                c.system_type_id IN(40, 41, 42, 43, 58, 61)
                                    THEN
                                        NULL
                            ELSE
                                ODBCSCALE(c.system_type_id, c.scale)
                        END
                )                                               AS NUMERIC_SCALE
        ,
            convert
                (
                    smallint
                    ,
                        CASE-- datetime / smalldatetime
                            WHEN
                                c.system_type_id IN(40, 41, 42, 43, 58, 61)
                                    THEN
                                        ODBCSCALE(c.system_type_id, c.scale)
                        END
                )                                               AS DATETIME_PRECISION
        , convert(nvarchar(30), null)                           AS INTERVAL_TYPE
        , convert(smallint, null)                               AS INTERVAL_PRECISION
        ,
            convert
                (
                    sysname
                    ,
                        CASE
                            WHEN
                                u.schema_id <> 4
                                    THEN
                                        DB_NAME()
                        END
                )                                               AS USER_DEFINED_TYPE_CATALOG
        ,
            convert
                (
                    sysname
                    ,
                        CASE
                            WHEN
                                u.schema_id <> 4
                                    THEN
                                        SCHEMA_NAME(u.schema_id)
                        END
                )                                               AS USER_DEFINED_TYPE_SCHEMA
        ,
            convert
                (
                    sysname
                    ,
                        CASE
                            WHEN
                                u.schema_id <> 4
                                    THEN
                                        u.name
                            END
                )                                               AS USER_DEFINED_TYPE_NAME
        , convert(sysname, null)                                AS SCOPE_CATALOG
        , convert(sysname, null) collate catalog_default        AS SCOPE_SCHEMA
        , convert(sysname, null) collate catalog_default        AS SCOPE_NAME
    FROM
        sys.all_objects o
            JOIN
                sys.all_parameters c
                    ON
                        c.object_id = o.object_id
            JOIN
                sys.types u
                    ON
                        u.user_type_id = c.user_type_id
    WHERE
        o.type IN('P', 'FN', 'TF', 'IF', 'IS', 'AF', 'PC', 'FS', 'FT')
        and
        o.name = @ProcedureName
)
        ";
        public override string ParametersQueryCommandText
        {
            get => _parametersQueryCommandText;
        }
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
            parameter
                .SqlDbType = SqlDbType.NVarChar;
            return
                parameter;
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
            var originalDbTypeName = (string) reader["DATA_TYPE"];
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
            
            if 
                (
                    Enum
                        .TryParse
                            (
                                dbTypeName
                                , true
                                , out SqlDbType dbType
                            )
                )
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
                                                    (int) o
                                                )
                                        )
                                        //& 255
                                    )
                            );
                }
                o = reader["NUMERIC_PRECISION"];
                if (o != DBNull.Value)
                {
                    parameter.Precision = (byte) o;
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
            return
                parameter;
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
                connection = new SqlConnection
                {
                    ConnectionString = connectionString
                };
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
                            , CommandType = CommandType.Text
                            , Connection = connection
                        }
                    )
                {
                    //command.CommandType = CommandType.StoredProcedure;
                    SqlParameter dbParameterUserDefinedTableTypeName = new SqlParameter
                    {
                        ParameterName = "@userDefinedTableTypeName",
                        Direction = ParameterDirection.Input,
                        Size = 128,
                        Value =
                                    (
                                        userDefinedTableTypeName != null
                                        ?
                                        (object)userDefinedTableTypeName
                                        :
                                        DBNull.Value
                                    ),
                        SqlDbType = SqlDbType.VarChar
                    };
                    command
                        .Parameters
                        .Add
                            (
                                dbParameterUserDefinedTableTypeName
                            );
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
                                    var columnName = (string) reader["ColumnName"];
                                    var columnTypeName = (string) reader["columnTypeName"];
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
