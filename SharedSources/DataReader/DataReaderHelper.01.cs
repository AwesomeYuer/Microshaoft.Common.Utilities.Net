namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;

    public static partial class DataReaderHelper
    {
        public static IEnumerable<T> ExecuteRead<T>
                        (
                            this IDataReader @this
                            , Func<int, IDataReader, T> onReadProcessFunc
                        )
        {
            try
            {
                int i = 0;
                while (@this.Read())
                {
                    if (onReadProcessFunc != null)
                    {
                        yield
                            return
                                onReadProcessFunc(++i, @this);
                    }
                }
            }
            finally
            {
                //可能有错 由于 yield 延迟
                @this.Close();
                @this.Dispose();
            }
        }
        public static IEnumerable<TEntry> GetEnumerable<TEntry>
                (
                    this IDataReader @this
                    , Func<IDataReader, TEntry> onReadProcessFunc
                    , bool skipNull = true
                )
                    where TEntry : new()
        {
            while (@this.Read())
            {
                var x = onReadProcessFunc(@this);
                if (!skipNull)
                {
                    yield
                        return
                                x;
                }
                else
                {
                    if (x != null)
                    {
                        yield
                           return
                                   x;
                    }
                }
            }
        }
        public static JArray GetColumnsJArray
                     (
                        this IDataReader @this
                     )
        {
            var fieldsCount = @this.FieldCount;
            HashSet<string> hashSet = null;
            JArray r = null;
            for (var i = 0; i < fieldsCount; i++)
            {
                if (r == null)
                {
                    r = new JArray();
                    hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
                var fieldType = @this.GetFieldType(i);
                var fieldName = @this.GetName(i);
                if (fieldName.IsNullOrEmptyOrWhiteSpace())
                {
                    fieldName = $"Column-{i + 1}";
                }
                while (hashSet.Contains(fieldName))
                {
                    fieldName += "_1";
                }
                hashSet.Add(fieldName);
                r.Add
                    (
                        new JObject
                                (
                                    new JProperty
                                        (
                                            "ColumnName"
                                            , fieldName
                                        )
                                    ,
                                    new JProperty
                                        (
                                            "Title"
                                            , fieldName
                                        )
                                    ,
                                    new JProperty
                                        (
                                            "title"
                                            , fieldName
                                        )
                                    ,
                                    new JProperty
                                        (
                                            "data"
                                            , fieldName
                                        )
                                    ,
                                    new JProperty
                                        (
                                            "ColumnType"
                                            , fieldType
                                                .GetJTokenType()
                                                .ToString()
                                        )
                                )
                    );
            }
            return r;
        }
        public static void ReadRows
                             (
                                 this IDataReader @this
                                 , JArray columns = null
                                 , Action
                                        <
                                            IDataReader
                                            , JArray        // columns
                                            , int           // row index
                                        >
                                            onReadRowProcessAction = null
                             )
        {
            var dbDataReader = (DbDataReader) @this;
            ReadRows
                (
                    dbDataReader
                    , columns
                    , onReadRowProcessAction
                );
        }

        public static void ReadRows
                             (
                                 this DbDataReader @this
                                 , JArray columns = null
                                 , Action
                                        <
                                            IDataReader
                                            , JArray        // columns
                                            , int           // row index
                                        >
                                            onReadRowProcessAction = null
                             )
        {
            //var fieldsCount = @this.FieldCount;
            int rowIndex = 0;
            if (columns == null)
            {
                columns = @this.GetColumnsJArray();
            }
            while (@this.Read())
            {
                onReadRowProcessAction?
                            .Invoke
                                (
                                    @this
                                    , columns
                                    , rowIndex
                                );
                rowIndex ++;
            }
        }

        public static async Task ReadRowsAsync
                             (
                                 this DbDataReader @this
                                 , JArray columns = null
                                 , Func
                                        <
                                            IDataReader
                                            , JArray        // columns
                                            , int           // row index
                                            , Task
                                        >
                                            onReadRowProcessActionAsync = null
                             )
        {
            int rowIndex = 0;
            if (columns == null)
            {
                columns = @this.GetColumnsJArray();
            }
            while
                (
                    await @this.ReadAsync()
                )
            {
                if (onReadRowProcessActionAsync != null)
                {
                    await
                        onReadRowProcessActionAsync
                                (
                                    @this
                                    , columns
                                    , rowIndex
                                );
                }
                rowIndex++;
            }
        }

        public static IEnumerable<JToken> AsRowsJTokensEnumerable
                             (
                                 this IDataReader @this
                                 , JArray columns = null
                                 , Func
                                        <
                                            //int             // resultSet index
                                            IDataReader
                                            , int           // row index
                                            , int           // column index
                                            , Type          // fieldType
                                            , string        // fieldName
                                            ,
                                                (
                                                    bool needDefaultProcess
                                                    , JProperty field   //  JObject Field 对象
                                                )
                                        >
                                            onReadRowColumnProcessFunc = null
                             )
        {
            var dbDataReader = (DbDataReader) @this;
            return
                AsRowsJTokensEnumerable
                    (
                        dbDataReader
                        , columns
                        , onReadRowColumnProcessFunc
                    );
        }
        public static IEnumerable<JToken> AsRowsJTokensEnumerable
                             (
                                 this DbDataReader @this
                                 , JArray columns = null
                                 , Func
                                        <
                                            //int             // resultSet index
                                            IDataReader
                                            , int           // row index
                                            , int           // column index
                                            , Type          // fieldType
                                            , string        // fieldName
                                            ,
                                                (
                                                    bool needDefaultProcess
                                                    , JProperty field   //  JObject Field 对象
                                                )
                                        >
                                            onReadRowColumnProcessFunc = null
                             )
        {
            var fieldsCount = @this.FieldCount;
            int rowIndex = 0;
            while (@this.Read())
            {
                JObject row = new JObject();
                for (var fieldIndex = 0; fieldIndex < fieldsCount; fieldIndex++)
                {
                    var fieldType = @this.GetFieldType(fieldIndex);
                    var fieldName = string.Empty;
                    if (columns != null)
                    {
                        fieldName = columns[fieldIndex]["ColumnName"].Value<string>();
                    }
                    else
                    {
                        @this.GetName(fieldIndex);
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
                                        @this
                                        , rowIndex
                                        , fieldIndex
                                        , fieldType
                                        , fieldName
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
                                        @this
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

        public static JProperty GetFieldJProperty
                            (
                                this IDataReader @this
                                , int i
                                , Type fieldType
                                , string fieldName
                            )
        {
            JValue fieldValue = null;
            if (!@this.IsDBNull(i))
            {
                if
                    (
                        fieldType == typeof(bool)
                    )
                {
                    fieldValue = new JValue(@this.GetBoolean(i));
                }
                else if
                    (
                        fieldType == typeof(byte)
                    )
                {
                    fieldValue = new JValue(@this.GetByte(i));
                }
                else if
                    (
                        fieldType == typeof(char)
                    )
                {
                    fieldValue = new JValue(@this.GetChar(i));
                }
                else if
                    (
                        fieldType == typeof(DateTime)
                    )
                {
                    fieldValue = new JValue(@this.GetDateTime(i));
                }
                else if
                    (
                        fieldType == typeof(decimal)
                    )
                {
                    fieldValue = new JValue(@this.GetDecimal(i));
                }
                else if
                    (
                        fieldType == typeof(double)
                    )
                {
                    fieldValue = new JValue(@this.GetDouble(i));
                }
                else if
                    (
                        fieldType == typeof(float)
                    )
                {
                    fieldValue = new JValue(@this.GetFloat(i));
                }
                else if
                    (
                        fieldType == typeof(Guid)
                    )
                {
                    fieldValue = new JValue(@this.GetGuid(i));
                }
                else if
                    (
                        fieldType == typeof(short)
                    )
                {
                    fieldValue = new JValue(@this.GetInt16(i));
                }
                else if
                    (
                        fieldType == typeof(int)
                    )
                {
                    fieldValue = new JValue(@this.GetInt32(i));
                }
                else if
                    (
                        fieldType == typeof(long)
                    )
                {
                    fieldValue = new JValue(@this.GetInt64(i));
                }
                else if
                    (
                        fieldType == typeof(string)
                    )
                {
                    fieldValue = new JValue(@this.GetString(i));
                }
                else
                {
                    fieldValue = new JValue(@this[i]);
                }
            }
            var r = new JProperty(fieldName, fieldValue);
            return r;
        }
    }
}