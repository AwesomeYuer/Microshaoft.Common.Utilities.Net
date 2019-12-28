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
            await foreach (var (rowIndex, columns, dataRecord) in entries)
            {
                yield
                    return
                        (
                            rowIndex
                            , columns
                            , dataRecord
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
                yield
                    return
                        (
                            resultSetIndex
                            , rowIndex
                            , columns
                            , dataRecord
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
            JArray jColumns = null; 
            while 
                (
                    await
                        target
                            .ReadAsync()
                )
            {
                TEntry entry = default;
                if (jColumns == null)
                {
                    jColumns = target
                                    .GetColumnsJArray();
                }
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
                var entries = target
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
                await
                    foreach
                        (
                            var
                                (
                                    rowIndex
                                    , columns
                                    , dataRecord
                                )
                            in
                            entries
                        )
                {
                    yield
                        return
                            (
                                resultSetIndex
                                , rowIndex
                                , columns
                                , dataRecord
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