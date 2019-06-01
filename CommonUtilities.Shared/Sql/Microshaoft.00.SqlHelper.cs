namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    public static partial class SqlHelper
    {
        public static JValue
                            GetJValue
                                <TDbParameter>
                                    (
                                        this DbParameter target
                                        , Func<TDbParameter, JValue>
                                                onDbParameterToJValueProcessFunc
                                    )
                            where
                                TDbParameter : DbParameter
        {
            JValue r = null;
            TDbParameter dbParameter = (TDbParameter)target;
            r = onDbParameterToJValueProcessFunc(dbParameter);
            return r;
        }
        public static TDbParameter ShallowClone<TDbParameter>
                        (
                            this DbParameter target
                            , Func<TDbParameter, TDbParameter, TDbParameter>
                                    onSetTypeProcessFunc
                            , bool includeValue = false
                        )
                            where
                                TDbParameter : DbParameter , new()
        {
            var clone = new TDbParameter();
            clone.ParameterName = target.ParameterName;
            clone.Size = target.Size;
            clone.Direction = target.Direction;
            clone.Scale = target.Scale;
            clone.Precision = target.Precision;
            if (includeValue)
            {
                //Shallow copy
                var targetValue = target.Value;
                if
                    (
                        targetValue != DBNull.Value
                        &&
                        targetValue != null
                    )
                {
                    DataTable dataTable = targetValue as DataTable;
                    if (dataTable != null)
                    {
                        clone.Value = dataTable.Clone();
                    }
                    else
                    {
                        //Shallow copy
                        clone.Value = target.Value;
                    }
                }
            }
            clone = onSetTypeProcessFunc
                            (
                                (TDbParameter) target
                                , clone
                            );
            return clone;
        }

        public static 
                IEnumerable<TDbParameter> 
                    GetStoreProcedureDefinitionParameters
                        <TDbConnection, TDbCommand, TDbParameter>
                            (
                                string connectionString
                                , string storeProcedureName
                                , Func<TDbParameter, TDbParameter>
                                        onQueryDefinitionsSetInputParameterProcessFunc
                                , Func<TDbParameter, TDbParameter>
                                        onQueryDefinitionsSetReturnParameterProcessFunc
                                , Func<IDataReader, TDbParameter, string, TDbParameter>
                                        onQueryDefinitionsReadOneDbParameterProcessFunc
                                , bool includeReturnValueParameter = false
                            )
                    where
                        TDbConnection : DbConnection, new ()
                    where
                        TDbCommand : DbCommand, new()
                    where
                        TDbParameter : DbParameter, new()
        {
            DbConnection connection = null;
            try
            {
                connection = new TDbConnection();
                connection.ConnectionString = connectionString;
                var dataSource = connection.DataSource;
                var dataBase = connection.Database;
                string procedureSchema = string.Empty;
                string parameterName = string.Empty;
                string parametersSchema = "information_schema.parameters";
                if (connection is SqlConnection)
                {
                    parametersSchema = MsSqlStoreProceduresParametersSchemaQuery;
                }
                var commandText = $@"
                    SELECT
                        * 
                    FROM
                        {parametersSchema} a 
                    WHERE
                        a.SPECIFIC_NAME = @ProcedureName
                    ";
                //MySQL 不支持 using command
                //using
                //    (
                DbCommand command = new TDbCommand()
                {
                    CommandText = commandText
                    , CommandType = CommandType.Text
                    , Connection = connection
                };
                    //)
                {
                    //command.CommandType = CommandType.StoredProcedure;
                    TDbParameter dbParameterProcedureName = new TDbParameter();
                    dbParameterProcedureName.ParameterName = "@ProcedureName";
                    dbParameterProcedureName.Direction = ParameterDirection.Input;
                    dbParameterProcedureName.Size = 128;
                    dbParameterProcedureName
                            .Value = 
                                    (
                                        storeProcedureName != null 
                                        ?
                                        (object)storeProcedureName
                                        :
                                        DBNull.Value
                                    );

                    dbParameterProcedureName 
                            = onQueryDefinitionsSetInputParameterProcessFunc
                                    (
                                        dbParameterProcedureName
                                    );
                    command.Parameters.Add(dbParameterProcedureName);
                    TDbParameter dbParameterReturn = new TDbParameter();
                    dbParameterReturn.ParameterName = "@RETURN_VALUE";
                    dbParameterReturn.Direction = ParameterDirection.ReturnValue;
                    dbParameterReturn 
                        = onQueryDefinitionsSetReturnParameterProcessFunc
                                (
                                    dbParameterReturn
                                );
                    connection.Open();
                    var dataReader
                            = command
                                    .ExecuteReader
                                        (
                                            CommandBehavior
                                                .CloseConnection
                                        );
                    var dbParameters
                            = dataReader
                                    .ExecuteRead
                                        (
                                            (x, reader) =>
                                            {
                                                var dbParameter = new TDbParameter();
                                                dbParameter
                                                    .ParameterName 
                                                        = (string)(reader["PARAMETER_NAME"]);
                                                if (reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value)
                                                {
                                                    dbParameter
                                                        .Size = reader
                                                                    .GetInt32
                                                                        (
                                                                            reader
                                                                                .GetOrdinal
                                                                                    ("CHARACTER_MAXIMUM_LENGTH")
                                                                        );
                                                }
                                                dbParameter
                                                    .Direction
                                                        = GetParameterDirection
                                                            (
                                                                reader
                                                                    .GetString
                                                                        (
                                                                            reader
                                                                                .GetOrdinal
                                                                                    ("PARAMETER_MODE")
                                                                        )
                                                            );
                                                var r =
                                                    onQueryDefinitionsReadOneDbParameterProcessFunc
                                                        (
                                                            reader
                                                            , dbParameter
                                                            , connectionString
                                                        );
                                                return r;
                                            }
                                        );
                    return dbParameters;
                }
            }
            finally
            {
                //connection.Close();
                //connection = null;
            }
        }
        public static ParameterDirection GetParameterDirection(string parameterMode)
        {
            ParameterDirection pd;
            if (string.Compare(parameterMode, "IN", true) == 0)
            {
                pd = ParameterDirection.Input;
            }
            else if (string.Compare(parameterMode, "INOUT", true) == 0)
            {
                pd = ParameterDirection.InputOutput;
            }
            else if (string.Compare(parameterMode, "RETURN", true) == 0)
            {
                pd = ParameterDirection.ReturnValue;
            }
            else
            {
                pd = ParameterDirection.Output;
            }
            return pd;
        }
    }
    public static partial class SqlHelper
    {
        private const string MsSqlStoreProceduresParametersSchemaQuery =
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
)
        ";
    }
}
