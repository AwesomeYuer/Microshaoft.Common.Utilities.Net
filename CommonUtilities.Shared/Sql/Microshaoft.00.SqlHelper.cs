namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    public static class SqlHelper
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
                var commandText = @"
                    SELECT
                        * 
                    FROM
                        --information_schema.parameters a 
                        zv_all_PARAMETERS a
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
}