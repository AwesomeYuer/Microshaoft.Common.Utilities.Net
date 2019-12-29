namespace MySqlConsoleApp
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Microshaoft;
    //using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    class Program
    {
        async static Task Main(string[] args)
        {
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

            var jTokenParameters = JToken.Parse(json);
            var spName = "usp_executesql";

            var x = new MsSqlStoreProceduresExecutor();

            SqlConnection sqlConnection = new SqlConnection
                (
                    "Initial Catalog=test;Data Source=gateway.hyper-v.internal\\sql2019,11433;User=sa;Password=!@#123QWE"
                )
            {
                StatisticsEnabled = true
            };

            x.CachedParametersDefinitionExpiredInSeconds = 10;

            var entries = x.ExecuteResultsAsAsyncEnumerable(sqlConnection, spName, jTokenParameters);
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

            //var xx = new MySqlStoreProceduresExecutor();
            //var mySqlConnection = new MySqlConnection()
            //{
            //    ConnectionString = "server=gateway.hyper-v.internal;uid=root;pwd=!@#123QWE;database=Test"
            //};

            //entries = xx.ExecuteResultsAsAsyncEnumerable
            //                (
            //                    mySqlConnection
            //                    , "zsp_test"
            //                    , JToken.Parse(@"{ Param1 : ""11""}")
            //                );

            //await foreach
            //        (
            //            var (
            //                    resultSetIndex
            //                    , rowIndex
            //                    , columns
            //                    , dataRecord
            //                )
            //            in
            //            entries
            //        )
            //{
            //    Console.WriteLine
            //                (
            //                    $"{nameof(resultSetIndex)}:{resultSetIndex}{{0}}{nameof(rowIndex)}:{rowIndex}{{0}}{nameof(dataRecord)}:{dataRecord[1]}"
            //                    , "\t"
            //                );

            //}

            Console.WriteLine("Hello World!");
        }
    }
}
