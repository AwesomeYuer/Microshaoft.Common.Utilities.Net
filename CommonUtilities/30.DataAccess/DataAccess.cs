namespace Microsoft.Boc
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using Microsoft.Boc.Communication.Configurations;
    public static class DataAccess
    {
        public static string _connectionString
                                = ConfigurationAppSettingsManager
                                    .RunTimeAppSettings
                                    .DataBaseConnectionString;
        private static DataTable _stringVarcharEmptyDataTable =
                    new Func<DataTable>
                        (
                            () =>
                            {
                                var type =
                                        new
                                            {
                                                ID = 0
                                                ,
                                                V = string.Empty
                                            }
                                            .GetType();
                                var dataTable = DataTableHelper.GenerateEmptyDataTable(type);
                                return dataTable;
                            }
                        )();

        public static void GetExclusiveServersSessionsPages
                        (
                            string[] p_ExclusiveServers
                            , int p_FetchRows
                            , Func<int, DataTable, bool> onPageProcessFunc
                        )
        {
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(_connectionString);
                using(var command = new SqlCommand("zsp_GetExclusiveServersSessions", connection))
	            {
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter parameter1 = command.Parameters.Add("@ExclusiveServers", SqlDbType.Structured);
                    var dataTable1 = _stringVarcharEmptyDataTable.Clone();
                    int i = 0;
                    Array
                        .ForEach
                            (
                                p_ExclusiveServers
                                , (x) =>
                                    {
                                        var dataRow = dataTable1.NewRow();
                                        dataRow["ID"] = ++i;
                                        dataRow["V"] = x;
                                        dataTable1.Rows.Add(dataRow);
                                    }
                            );
                    parameter1.Value = (p_ExclusiveServers != null ? (object)dataTable1 : DBNull.Value);
                    command.ExecuteDataTablePager
                            (
                                p_FetchRows
                                , onPageProcessFunc
                            );  
	            }
            }
            finally
            {
                connection.Close();
                //connection = null;
            }
        }
        public static long InsertOneMessageForOneReceiverEntry
                            (
                                string p_ReceiverAppID
                                , string p_ReceiverGroupID
                                , string p_ReceiverUserID
                                , string p_SenderAppID
                                , string p_SenderGroupID
                                , string p_SenderUserID
                                , bool p_NeedTrace
                                , string p_Topic
                                , string p_Message
                                , int p_status
                                , DateTime? p_ExpireTime
                                , long p_MessageID
                                //, out long p_ID
                            )
        {
            SqlConnection connection = null;
            //long p_MessageID = -1;
            long p_ID = -1;
            try
            {
                connection = new SqlConnection(_connectionString);
                using (var command = new SqlCommand("zsp_InsertMessage", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter parameter1 = command.Parameters.Add("@ReceiverAppID", SqlDbType.VarChar, 50);
                    parameter1.Value = (p_ReceiverAppID != null ? (object)p_ReceiverAppID : DBNull.Value);
                    SqlParameter parameter2 = command.Parameters.Add("@ReceiverGroupID", SqlDbType.VarChar, 50);
                    parameter2.Value = (p_ReceiverGroupID != null ? (object)p_ReceiverGroupID : DBNull.Value);
                    SqlParameter parameter3 = command.Parameters.Add("@ReceiverUserID", SqlDbType.VarChar, 50);
                    parameter3.Value = (p_ReceiverUserID != null ? (object)p_ReceiverUserID : DBNull.Value);
                    SqlParameter parameter4 = command.Parameters.Add("@SenderAppID", SqlDbType.VarChar, 50);
                    parameter4.Value = (p_SenderAppID != null ? (object)p_SenderAppID : DBNull.Value);
                    SqlParameter parameter5 = command.Parameters.Add("@SenderGroupID", SqlDbType.VarChar, 50);
                    parameter5.Value = (p_SenderGroupID != null ? (object)p_SenderGroupID : DBNull.Value);
                    SqlParameter parameter6 = command.Parameters.Add("@SenderUserID", SqlDbType.VarChar, 50);
                    parameter6.Value = (p_SenderUserID != null ? (object)p_SenderUserID : DBNull.Value);

                    SqlParameter parameter7 = command.Parameters.Add("@NeedTrace", SqlDbType.Bit);
                    parameter7.Value = p_NeedTrace;

                    SqlParameter parameter8 = command.Parameters.Add("@Topic", SqlDbType.VarChar, 50);
                    parameter8.Value = (p_Topic != null ? (object)p_Topic : DBNull.Value);

                    SqlParameter parameter9 = command.Parameters.Add("@Message", SqlDbType.VarChar, int.MaxValue);
                    parameter9.Value = (p_Message != null ? (object)p_Message : DBNull.Value);

                    SqlParameter parameter10 = command.Parameters.Add("@status", SqlDbType.TinyInt);
                    parameter10.Value = p_status;

                    SqlParameter parameter11 = command.Parameters.Add("@ExpireTime", SqlDbType.DateTime);
                    parameter11.Value = (p_ExpireTime != null ? (object)p_ExpireTime : DBNull.Value);

                    SqlParameter parameter12 = command.Parameters.Add("@MessageID", SqlDbType.BigInt);
                    parameter12.Direction = ParameterDirection.InputOutput;
                    parameter12.Value = p_MessageID;

                    SqlParameter parameter13 = command.Parameters.Add("@ID", SqlDbType.BigInt);
                    parameter13.Direction = ParameterDirection.Output;
                    connection.Open();
                    command.ExecuteNonQuery();
                    if (parameter12.Value != DBNull.Value)
                    {
                        p_MessageID = ((long)(parameter12.Value));
                    }

                    if (parameter13.Value != DBNull.Value)
                    {
                        p_ID = ((long)(parameter13.Value));
                    } 
                }
                return (p_MessageID);
            }
            finally
            {
                connection.Close();
                connection = null;
            }

        }
        public static void UpdateMessagesStatus(int p_status, DataTable p_Messages)
        {
            SqlConnection connection = null;// new SqlConnection(_connectionString);
            try
            {
                connection = new SqlConnection(_connectionString);
                using (SqlCommand command = new SqlCommand("zsp_UpdateMessagesStatus", connection))
                {
                    
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter parameter1 = command.Parameters.Add("@status", SqlDbType.TinyInt);
                    parameter1.Value = p_status;
                    SqlParameter parameter2 = command.Parameters.Add("@Messages", SqlDbType.Structured);
                    parameter2.Value = (p_Messages != null ? (object)p_Messages : DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery(); 
                }
            }
            finally
            {
                connection.Close();
                connection = null;
            }
        }
        public static void UpdateSessions(DataTable p_Sessions)
        {
            SqlConnection connection = null;// new SqlConnection(_connectionString);
            try
            {
                connection = new SqlConnection(_connectionString);
                using (SqlCommand command = new SqlCommand("zsp_MergeCurrentSessions", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter parameter1 = command.Parameters.Add("@Sessions", SqlDbType.Structured);
                    parameter1.Value = (p_Sessions != null ? (object)p_Sessions : DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery(); 
                }
            }
            finally
            {
                connection.Close();
                connection = null;
            }
        }
    }
}
