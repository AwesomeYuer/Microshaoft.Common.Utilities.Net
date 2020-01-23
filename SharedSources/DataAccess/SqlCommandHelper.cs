namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    public static class SqlCommandHelper
    {
        public static IEnumerable<TEntry> ExecuteRead<TEntry>
                (
                    this SqlCommand target
                    , Func<int, IDataReader, TEntry> onReadProcessFunc
                )
        {
            SqlConnection sqlConnection = target.Connection;
            if (sqlConnection.State == ConnectionState.Closed)
            {
                sqlConnection.Open();
            }
            //using
            //    (
            IDataReader dataReader
                            = target
                                    .ExecuteReader
                                            (
                                                CommandBehavior.CloseConnection
                                            );
            //)
            //{
            return
                dataReader
                    .ExecuteRead<TEntry>
                        (
                            onReadProcessFunc
                        );
            //}

        }

        public static void ExecuteDataReader
               (
                   this SqlCommand target
                   , Func<int, IDataReader, bool> onReadProcessFunc
               )
        {
            var needBreak = false;
            SqlConnection sqlConnection = null;
            try
            {
                sqlConnection = target.Connection;
                if (sqlConnection.State == ConnectionState.Closed)
                {
                    sqlConnection.Open();
                }

                using
                    (
                        IDataReader dataReader
                                        = target
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
                    this SqlCommand target
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
            target.Parameters["@Top"].Value = pageFetchRows;
            do
            {
                //页码
                page++;
                var sqlConnection = target.Connection;
                sqlConnection.Open();
                using (IDataReader dataReader = target.ExecuteReader(CommandBehavior.CloseConnection))
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
                var parameterIsLast = target.Parameters["@IsLast"];
                if
                    (
                        parameterIsLast.Value != null
                        &&
                        parameterIsLast.Value != DBNull.Value
                    )
                {
                    target.Parameters["@LeftID"].Value = id;
                    isLast = (bool)parameterIsLast.Value;
                }
                else
                {
                    break;
                }
            }
            while (!isLast);
        }
        //[Conditional("NETFRAMEWORK4_X")]

        public static void ExecuteDataTablePager
        (
            this SqlCommand target
            , int pageFetchRows
            , Func<int, DataTable, bool> onPageProcessFunc
        )
        {
            SqlConnection connection = null;
            try
            {
                connection = target.Connection;
                var parameterOffsetRows = target.Parameters.Add("@OffsetRows", SqlDbType.Int);
                var parameterFetchRows = target.Parameters.Add("@FetchRows", SqlDbType.Int);
                var parameterTotalRows = target.Parameters.Add("@TotalRows", SqlDbType.Int);
                parameterTotalRows.Direction = ParameterDirection.InputOutput;
                var parameterIsLastPage = target.Parameters.Add("@IsLastPage", SqlDbType.Bit);
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
                    using (var sqlDataAdapter = new SqlDataAdapter(target))
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
