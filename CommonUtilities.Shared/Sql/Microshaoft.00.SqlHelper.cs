namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;

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
            var dbParameter = (TDbParameter) target;
            return
                onDbParameterToJValueProcessFunc(dbParameter);
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
            var clone = new TDbParameter
            {
                ParameterName = target.ParameterName
                , Size = target.Size
                , Direction = target.Direction
                , Scale = target.Scale
                , Precision = target.Precision
            };
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
                    if (targetValue is DataTable dataTable)
                    {
                        clone.Value = dataTable.Clone();
                    }
                    else
                    {
                        //Shallow copy
                        clone.Value = targetValue;
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
                                , string parametersQueryCommandText //= null
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
                connection = new TDbConnection
                {
                    ConnectionString = connectionString
                };
                var dataSource = connection.DataSource;
                var dataBase = connection.Database;
                string procedureSchema = string.Empty;
                string parameterName = string.Empty;
                DbCommand command = new TDbCommand()
                {
                    CommandText = parametersQueryCommandText
                    , CommandType = CommandType.Text
                    , Connection = connection
                };
                {
                    TDbParameter dbParameterProcedureName = new TDbParameter
                    {
                        ParameterName = "@ProcedureName"
                        ,Direction = ParameterDirection.Input
                        ,Size = 128
                        ,Value =
                            (
                                storeProcedureName != null
                                ?
                                (object) storeProcedureName
                                :
                                DBNull.Value
                            )
                    };
                    dbParameterProcedureName 
                            = onQueryDefinitionsSetInputParameterProcessFunc
                                    (
                                        dbParameterProcedureName
                                    );
                    command
                        .Parameters
                        .Add
                            (
                                dbParameterProcedureName
                            );
                    TDbParameter dbParameterReturn = new TDbParameter
                    {
                        ParameterName = "@RETURN_VALUE"
                        , Direction = ParameterDirection.ReturnValue
                    };
                    dbParameterReturn 
                        = onQueryDefinitionsSetReturnParameterProcessFunc
                                (
                                    dbParameterReturn
                                );
                    connection.Open();
                    //Npgsql should be use
                    //OpenAsync().Wait();
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
                                                        = (string) reader["PARAMETER_NAME"];
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
                    return
                        dbParameters;
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
            ParameterDirection @return;
            if (string.Compare(parameterMode, "IN", true) == 0)
            {
                @return = ParameterDirection.Input;
            }
            else if (string.Compare(parameterMode, "INOUT", true) == 0)
            {
                @return = ParameterDirection.InputOutput;
            }
            else if (string.Compare(parameterMode, "RETURN", true) == 0)
            {
                @return = ParameterDirection.ReturnValue;
            }
            else
            {
                @return = ParameterDirection.Output;
            }
            return @return;
        }
    }
}
