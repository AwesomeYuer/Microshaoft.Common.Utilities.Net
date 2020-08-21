namespace ConsoleApp
{
    using Microshaoft;
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class Program
    {
        async static Task Main(string[] args)
        {
            ValueTupleDataTableTest();

            Console.WriteLine("press any key to continue ...");

            Console.ReadLine();
            SingleThreadAsyncDequeueProcessorSlimTest();

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

            var executor = new MsSqlStoreProceduresExecutor(executingCachingStore);

            SqlConnection sqlConnection = new SqlConnection
                (
                    "Initial Catalog=test;Data Source=gateway.hyper-v.internal\\sql2019,11433;User=sa;Password=!@#123QWE"
                )
            {
                StatisticsEnabled = true
            };

            executor.CachedParametersDefinitionExpiredInSeconds = 10;

            var entries = executor
                                .ExecuteResultsAsAsyncEnumerable
                                    (
                                        sqlConnection
                                        , spName
                                        , jTokenParameters
                                    );
            await
                foreach
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
                Console
                    .WriteLine
                            (
                                $"{nameof(resultSetIndex)}:{resultSetIndex}{{0}}{nameof(rowIndex)}:{rowIndex}{{0}}{nameof(dataRecord)}:{dataRecord[1]}"
                                , "\t"
                            );
            }

            Console.WriteLine("press any key to continue ...");
            Console.ReadLine();

            var result = await
                                executor
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
                        var 
                            (
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

        private static void SingleThreadAsyncDequeueProcessorSlimTest()
        {
            Console.WriteLine("SingleThreadAsyncDequeueProcessorSlim DataTable Test:");
            var processor = new SingleThreadAsyncDequeueProcessorSlim<(int id, string text, DateTime time)>();
            var dataTable = typeof((int, string, DateTime, long, long))
                                        .GenerateEmptyDataTable
                                                ("F1", "F2", "F3", "EnqueueTimestamp", "DequeueTimestamp");

            var jArray = new JArray();
            processor
                .StartRunDequeueThreadProcess
                    (
                        (dequeued, batch, indexInBatch, queueElement) =>
                        {
                            var (id, text, time) = queueElement.Element;
                            dataTable
                                    .Rows
                                    .Add
                                        (
                                            id
                                            , text
                                            , time
                                            , queueElement
                                                        .Timing
                                                        .EnqueueTimestamp
                                            , queueElement
                                                        .Timing
                                                        .DequeueTimestamp
                                        );

                            jArray
                                .Add
                                    (
                                        new JObject
                                        {
                                              { "id" , id }
                                            , { "text" , text }
                                            , { "time" , time }
                                            , { "EnqueueTimestamp", queueElement.Timing.EnqueueTimestamp }
                                            , { "DequeueTimestamp", queueElement.Timing.DequeueTimestamp }
                                        }
                                    );
                                   
                        }
                        , (dequeued, batch, indexInBatch) =>
                        {
                            try
                            {
                                #region use DataTable
                                var dataRows = dataTable.AsEnumerable();
                                var enqueueTimestamp = dataRows
                                                                .Min
                                                                    (
                                                                        (x) =>
                                                                        {
                                                                            return
                                                                                x.Field<long>("EnqueueTimestamp");
                                                                        }
                                                                    );
                                var dequeueTimestamp = dataRows
                                                                .Max
                                                                    (
                                                                        (x) =>
                                                                        {
                                                                            return
                                                                                x.Field<long>("DequeueTimestamp");
                                                                        }
                                                                    );
                                var durationInQueue = enqueueTimestamp.GetElapsedTime(dequeueTimestamp).TotalMilliseconds;
                                Console.WriteLine($"{nameof(durationInQueue)}:{durationInQueue};{nameof(dequeued)}:{dequeued};{nameof(batch)}:{batch};{nameof(indexInBatch)}:{indexInBatch};{nameof(dataTable.Rows.Count)}:{dataTable.Rows.Count}");
                                var dataColumns = dataTable.Columns;
                                foreach (var dataRow in dataRows)
                                {
                                    foreach (DataColumn dataColumn in dataColumns)
                                    {
                                        var columnName = dataColumn.ColumnName;
                                        //Console.Write($"{columnName}:{dataRow[columnName]}\t");
                                    }
                                    //Console.Write("\n");
                                }
                                #endregion

                                #region use JArray
                                enqueueTimestamp = jArray
                                                        .Min
                                                            (
                                                                (x) =>
                                                                {
                                                                    return
                                                                        x["EnqueueTimestamp"].Value<long>();
                                                                }
                                                            );
                                dequeueTimestamp = jArray
                                                        .Max
                                                            (
                                                                (x) =>
                                                                {
                                                                    return
                                                                        x["DequeueTimestamp"].Value<long>();
                                                                }
                                                            );
                                durationInQueue = enqueueTimestamp.GetElapsedTime(dequeueTimestamp).TotalMilliseconds;
                                Console.WriteLine($"{nameof(durationInQueue)}:{durationInQueue};{nameof(dequeued)}:{dequeued};{nameof(batch)}:{batch};{nameof(indexInBatch)}:{indexInBatch};{nameof(jArray.Count)}:{jArray.Count}"); 
                                #endregion
                            }
                            finally
                            {
                                dataTable.Clear();
                                jArray.Clear();
                            }
                        }
                        , 200
                        , 1000
                        , 1100
                    );
            for (var i = 0; i < 10000; i++)
            {
                processor.Enqueue((i, $"No.{i} Element", DateTime.Now));
                Thread.Sleep(10);
            }
            
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
