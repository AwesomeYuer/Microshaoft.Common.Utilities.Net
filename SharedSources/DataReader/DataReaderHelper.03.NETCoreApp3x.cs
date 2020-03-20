#if NETCOREAPP3_X || NETSTANDARD2_X
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
                    this DbDataReader @this
            )
        {
            var entries = @this
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
                    this DbDataReader @this
            )
        {
            var entries = @this
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
                    this DbDataReader @this
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
                        @this
                            .ReadAsync()
                )
            {
                TEntry entry = default;
                if (jColumns == null)
                {
                    jColumns = @this
                                    .GetColumnsJArray();
                }
                if (onEntryFactoryProcessFunc != null)
                {
                    entry = onEntryFactoryProcessFunc(rowIndex, jColumns, @this);
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
                    this DbDataReader @this
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
                var entries = @this
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
                        @this
                            .NextResultAsync()
                );
        }
    }
}
#endif