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
        public static async IAsyncEnumerable<JToken> AsRowsJTokensEnumerableAsync
                     (
                         this IDataReader target
                         , JArray columns = null
                         , Func
                                <
                                    IDataReader
                                    , Type          // fieldType
                                    , string        // fieldName
                                    , int           // row index
                                    , int           // column index
                                    ,
                                        (
                                            bool needDefaultProcess
                                            , JProperty field   //  JObject Field 对象
                                        )
                                >
                                    onReadRowColumnProcessFunc = null
                     )
        {
            var dbDataReader = (DbDataReader) target;
            var items = AsRowsJTokensEnumerableAsync
                            (
                                dbDataReader
                                , columns
                                , onReadRowColumnProcessFunc
                            );
            await foreach (var item in items)
            {
                yield
                    return
                        item;
            }
        }
        public static async IAsyncEnumerable<JToken> AsRowsJTokensEnumerableAsync
                     (
                         this DbDataReader target
                         , JArray columns = null
                         , Func
                                <
                                    IDataReader
                                    , Type          // fieldType
                                    , string        // fieldName
                                    , int           // row index
                                    , int           // column index
                                    ,
                                        (
                                            bool needDefaultProcess
                                            , JProperty field   //  JObject Field 对象
                                        )
                                >
                                    onReadRowColumnProcessFunc = null
                     )
        {
            var fieldsCount = target.FieldCount;
            int rowIndex = 0;
            while (await target.ReadAsync())
            {
                JObject row = new JObject();
                for (var fieldIndex = 0; fieldIndex < fieldsCount; fieldIndex++)
                {
                    var fieldType = target.GetFieldType(fieldIndex);
                    var fieldName = string.Empty;
                    if (columns != null)
                    {
                        fieldName = columns[fieldIndex]["ColumnName"].Value<string>();
                    }
                    else
                    {
                        target.GetName(fieldIndex);
                        if (fieldName.IsNullOrEmptyOrWhiteSpace())
                        {
                            fieldName = $"Column-{fieldIndex + 1}";
                        }
                    }
                    JProperty field = null;
                    var needDefaultProcess = true;
                    if (onReadRowColumnProcessFunc != null)
                    {
                        var r = onReadRowColumnProcessFunc
                                    (
                                        target
                                        , fieldType
                                        , fieldName
                                        , rowIndex
                                        , fieldIndex
                                    );
                        needDefaultProcess = r.needDefaultProcess;
                        if (r.field != null)
                        {
                            field = r.field;
                        }
                    }
                    if (needDefaultProcess)
                    {
                        field = GetFieldJProperty
                                    (
                                        target
                                        , fieldIndex
                                        , fieldType
                                        , fieldName
                                    );
                    }
                    if (field != null)
                    {
                        row.Add(field);
                    }
                }
                rowIndex ++;
                yield
                    return
                            row;
            }
        }
       
    }
}
#endif