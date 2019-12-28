#if NETCOREAPP3_X
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;

    public static partial class DataReaderHelper
    {
        public static async
                        IAsyncEnumerable
                                <
                                    (
                                        int         // rowIndex
                                        , JArray    // columns
                                        , IDataRecord
                                    )
                                >
                                    AsOneResultIAsyncEnumerable
            (
                    this DbDataReader target
            )
        {
            var entries = target
                                .AsOneResultIAsyncEnumerable<IDataRecord>
                                    (
                                        (rowIndex, columns, dataRecord) =>
                                        {
                                            return dataRecord;
                                        }
                                    );
            await foreach (var entry in entries)
            {
                yield
                    return
                        (
                            entry.Item1
                            , entry.Item2
                            , entry.Item3
                        );
            }
        }
        public static async
                        IAsyncEnumerable
                                <
                                    (
                                          int       //resultSetIndex
                                        , int       // rowIndex
                                        , JArray    // columns
                                        , IDataRecord
                                    )
                                >
                                    AsMultipleResultsIAsyncEnumerable
            (
                    this DbDataReader target
            )
        {
            var entries = target
                                .AsMultipleResultsIAsyncEnumerable<IDataRecord>
                                    (
                                        (resultSetIndex, rowIndex, columns, dataRecord) =>
                                        {
                                            return dataRecord;
                                        }
                                    );
            await foreach (var entry in entries)
            {
                yield
                    return
                        (
                            entry.Item1
                            , entry.Item2
                            , entry.Item3
                            , entry.Item4
                        );
            }
        }

        public static async
                        IAsyncEnumerable
                                <
                                    (
                                        int         // rowIndex
                                        , JArray    // columns
                                        , TEntry
                                    )
                                >
                                    AsOneResultIAsyncEnumerable<TEntry>
            (
                    this DbDataReader target
                    , Func
                        <
                            int
                            , JArray
                            , IDataRecord
                            , TEntry
                        >
                            onEntryFactoryProcessFunc = null
            )
        {
            var rowIndex = 0;
            while 
                (
                    await
                        target
                            .ReadAsync()
                )
            {
                TEntry entry = default;
                var jColumns = target
                                    .GetColumnsJArray();
                if (onEntryFactoryProcessFunc != null)
                {
                    entry = onEntryFactoryProcessFunc(rowIndex, jColumns, target);
                }
                yield
                    return
                       (rowIndex, jColumns, entry);
                rowIndex ++;
            }
        }
        public static async IAsyncEnumerable
                            <
                                (
                                    int             // resultSetIndex
                                    , int           // rowIndex
                                    , JArray        // columns
                                    , TEntry
                                )
                            >
                                AsMultipleResultsIAsyncEnumerable<TEntry>
            (
                    this DbDataReader target
                    , Func
                            <
                                int
                                , int
                                , JArray
                                , IDataRecord
                                , TEntry
                            >
                                onEntryFactoryProcessFunc = null
            )
        {
            int resultSetIndex = 0;
            do
            {
                var rows = target
                                .AsOneResultIAsyncEnumerable //<TEntry>
                                    (
                                        (rowIndex, columns, dataRecord) =>
                                        {
                                            TEntry r = default;
                                            if (onEntryFactoryProcessFunc != null)
                                            {
                                                r = onEntryFactoryProcessFunc
                                                                (
                                                                    resultSetIndex
                                                                    , rowIndex
                                                                    , columns
                                                                    , dataRecord
                                                                );
                                            }
                                            return r;
                                        }
                                    );
                await foreach (var row in rows)
                {
                    yield
                        return
                            (
                                resultSetIndex
                                , row.Item1
                                , row.Item2
                                , row.Item3
                            );
                }
                resultSetIndex ++;
            }
            while
                (
                    await
                        target
                            .NextResultAsync()
                );
        }
    }
}
#endif