#if !NETSTANDARD1_4
namespace Microshaoft
{
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public static class SqlCommandExtensionsMethodsManager
    {
        public static void ExecuteDataReader
                (
                    this SqlCommand command
                    , Func<int, IDataReader, bool> onReadProcessFunc
                )
        {
            var needBreak = false;
            SqlConnection sqlConnection = null;
            try
            {
                sqlConnection = command.Connection;
                if (sqlConnection.State == ConnectionState.Closed)
                {
                    sqlConnection.Open();
                }

                using
                    (
                        IDataReader dataReader
                                        = command
                                                .ExecuteReader
                                                        (
                                                            CommandBehavior.CloseConnection
                                                        )
                    )
                {
                    int i = 0;
                    while (dataReader.Read())
                    {
                        if (onReadProcessFunc != null)
                        {
                            needBreak = onReadProcessFunc(++i, dataReader);
                        }
                        if (needBreak)
                        {
                            break;
                        }
                    }
                    dataReader.Close();
                }
            }
            finally
            {
                if (sqlConnection.State != ConnectionState.Closed)
                {
                    sqlConnection.Close();
                }
            }
        }
        public static void ExecuteDataReaderPager
                (
                    this SqlCommand command
                    , int pageFetchRows
                    , string idColumnName
                    , Func<int, int, int, IDataReader, bool>
                            onReadProcessFunc
                )
        {
            //总序号
            var i = 0;
            //页码
            var page = 0;
            var needBreak = false;
            var isLast = false;
            var id = 0L;
            command.Parameters["@Top"].Value = pageFetchRows;
            do
            {
                //页码
                page++;
                var sqlConnection = command.Connection;
                sqlConnection.Open();
                using (IDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    int ii = 0; //本页序号
                    while (dataReader.Read())
                    {
                        id = (long)dataReader[idColumnName];
                        if (onReadProcessFunc != null)
                        {
                            needBreak = onReadProcessFunc(++i, page, ++ii, dataReader);
                        }
                        if (needBreak)
                        {
                            break;
                        }
                    }
                    dataReader.Close();
                }
                if (needBreak)
                {
                    break;
                }
                var parameterIsLast = command.Parameters["@IsLast"];
                if
                    (
                        parameterIsLast.Value != null
                        &&
                        parameterIsLast.Value != DBNull.Value
                    )
                {
                    command.Parameters["@LeftID"].Value = id;
                    isLast = (bool)parameterIsLast.Value;
                }
                else
                {
                    break;
                }
            }
            while (!isLast);
        }
        public static void ExecuteDataTablePager
                (
                    this SqlCommand command
                    , int pageFetchRows
                    , Func<int, DataTable, bool> onPageProcessFunc
                )
        {
            SqlConnection connection = null;
            try
            {
                connection = command.Connection;
                var parameterOffsetRows = command.Parameters.Add("@OffsetRows", SqlDbType.Int);
                var parameterFetchRows = command.Parameters.Add("@FetchRows", SqlDbType.Int);
                var parameterTotalRows = command.Parameters.Add("@TotalRows", SqlDbType.Int);
                parameterTotalRows.Direction = ParameterDirection.InputOutput;
                var parameterIsLastPage = command.Parameters.Add("@IsLastPage", SqlDbType.Bit);
                parameterIsLastPage.Direction = ParameterDirection.Output;
                int p_OffsetRows = 0;
                bool p_IsLast = false;
                int p_TotalRows = -1;
                int page = 0;
                parameterFetchRows.Value = pageFetchRows;
                DataTable dataTable = null;
                while (!p_IsLast)
                {
                    parameterOffsetRows.Value = p_OffsetRows;
                    parameterFetchRows.Value = pageFetchRows;
                    using (var sqlDataAdapter = new SqlDataAdapter(command))
                    {
                        using (var dataSet = new DataSet())
                        {
                            sqlDataAdapter.Fill(dataSet);
                            dataTable = dataSet.Tables[0];
                            if (parameterTotalRows.Value != DBNull.Value)
                            {
                                p_TotalRows = ((int)(parameterTotalRows.Value));
                            }
                            if (parameterIsLastPage.Value != DBNull.Value)
                            {
                                p_IsLast = ((bool)(parameterIsLastPage.Value));
                            }
                            //connection.Close();
                        }
                    }
                    var r = false;
                    if (dataTable != null)
                    {
                        if (dataTable.Rows.Count > 0)
                        {
                            r = onPageProcessFunc(++page, dataTable);
                        }
                    }
                    if (r)
                    {
                        break;
                    }
                    p_OffsetRows += pageFetchRows;
                }
            }
            finally
            {
                //connection.Close();
            }
        }
    }
}

#endif
