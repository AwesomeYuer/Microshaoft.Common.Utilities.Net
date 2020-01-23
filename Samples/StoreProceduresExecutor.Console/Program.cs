namespace ConsoleApp
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Microshaoft;
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using System.Linq;
    using System.Collections.Concurrent;

    public static class Program
    {
        async static Task Main(string[] args)
        {

            ValueTupleDataTableTest();

            Console.WriteLine("press any key to continue ...");

            Console.ReadLine();

            Console.WriteLine("use MSSQL ...");
            var json =
            @"
{
    sql:
        ""
            set statistics io on
            set statistics time on
            set statistics profile on
            select
                '\""111\""' as F1, *
            from
                sys.objects

            select
                '\""222\""' as F, *
            from
                sys.objects
        ""
}
                        ";

            json = @"
        {
            a: [
                {F1:""asdsa"", F2: 123, F3: ""2019-01-01""}
            ]
        }

";
            var jTokenParameters = JToken.Parse(json);
            var spName = "usp_executesql";
            spName = "usp_testudt";
            ConcurrentDictionary<string, ExecutingInfo>
                        executingCachingStore
                                = new ConcurrentDictionary<string, ExecutingInfo>();

            var x = new MsSqlStoreProceduresExecutor(executingCachingStore);

            SqlConnection sqlConnection = new SqlConnection
                (
                    "Initial Catalog=test;Data Source=gateway.hyper-v.internal\\sql2019,11433;User=sa;Password=!@#123QWE"
                )
            {
                StatisticsEnabled = true
            };

            x.CachedParametersDefinitionExpiredInSeconds = 10;

            var entries = x
                    .ExecuteResultsAsAsyncEnumerable
                        (
                            sqlConnection
                            , spName
                            , jTokenParameters
                        );
            await foreach
                    (
                        var (
                                resultSetIndex
                                , rowIndex
                                , columns
                                , dataRecord
                            )
                        in
                        entries
                    )
            {
                Console.WriteLine
                            (
                                $"{nameof(resultSetIndex)}:{resultSetIndex}{{0}}{nameof(rowIndex)}:{rowIndex}{{0}}{nameof(dataRecord)}:{dataRecord[1]}"
                                , "\t"
                            );

            }

            Console.WriteLine("press any key to continue ...");
            Console.ReadLine();

            var result = await
                        x
                            .ExecuteJsonResultsAsync
                                (
                                    sqlConnection
                                    , spName
                                    , jTokenParameters
                                );

            Console.WriteLine(result);

            Console.WriteLine("press any key to continue ...");
            Console.ReadLine();

            Console.WriteLine("use MySQL ...");
            ConcurrentDictionary<string, ExecutingInfo> store = new ConcurrentDictionary<string, ExecutingInfo>();
            var xx = new MySqlStoreProceduresExecutor(store);
            var mySqlConnection = new MySqlConnection()
            {
                ConnectionString = "server=gateway.hyper-v.internal;uid=root;pwd=!@#123QWE;database=Test"
            };

            entries = xx.ExecuteResultsAsAsyncEnumerable
                            (
                                mySqlConnection
                                , "zsp_test"
                                , JToken.Parse(@"{ Param1 : ""11""}")
                            );

            await foreach
                    (
                        var (
                                resultSetIndex
                                , rowIndex
                                , columns
                                , dataRecord
                            )
                        in
                        entries
                    )
            {
                Console.WriteLine
                            (
                                $"{nameof(resultSetIndex)}:{resultSetIndex}{{0}}{nameof(rowIndex)}:{rowIndex}{{0}}{nameof(dataRecord)}:{dataRecord[1]}"
                                , "\t"
                            );

            }

            Console.WriteLine("Hello World!");
        }

        private static void ValueTupleDataTableTest()
        {
            Console.WriteLine("ValueTuple DataTable Test:");
            
            SqlConnection sqlConnection = new SqlConnection
                (
                    "Initial Catalog=test;Data Source=gateway.hyper-v.internal\\sql2019,11433;User=sa;Password=!@#123QWE"
                );
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = "usp_testudt";
            sqlCommand.CommandType = CommandType.StoredProcedure;
            var sqlParameter = new SqlParameter("a", SqlDbType.Structured);
            sqlCommand.Connection = sqlConnection;
            sqlCommand.Parameters.Add(sqlParameter);

            DataTable dataTable;
            IDataReader dataReader;

            (string F1, int F2, DateTime F3) x = ("asdsad", 100, DateTime.Now);


            dataTable = x.GenerateEmptyDataTable(nameof(x.F1), "FF2");
            dataTable.Rows.Add(x.F1, x.F2, x.F3);
            sqlParameter.Value = dataTable;
            sqlConnection.Open();
            dataReader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
            while (dataReader.Read())
            {
                Console.WriteLine(dataReader.FieldCount);
            }
            dataReader.Close();
            //=================================================================
            (string F1, int F2, DateTime F3, (string F4, int F5, DateTime F6)) = ("asdsad", 100, DateTime.Now, ("asdsad", 100, DateTime.Now));

            var dataTable2 = typeof((string, int, DateTime, (string, int, DateTime)))
                                    .GenerateEmptyDataTable("_X", nameof(F1),"asdsa", "asdsa")
                                    ;
            dataTable2.Rows.Add(F1, F2, F3);

            sqlParameter.Value = dataTable;
            sqlConnection.Open();
            dataReader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
            while (dataReader.Read())
            {
                Console.WriteLine(dataReader.FieldCount);
            }
            dataReader.Close();



            sqlConnection.Close();
        }
    }
}
