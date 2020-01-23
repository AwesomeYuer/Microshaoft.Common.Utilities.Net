namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    public static partial class DbParameterHelper
    {
        public static object SetGetValueAsObject
                                    (
                                        this SqlParameter target
                                        , JToken jValue 
                                    )
        {
            object @return = null;
            if
                (
                    target.SqlDbType == SqlDbType.Structured
                )
            {
                var parameterValue = target.Value;
                if
                    (
                        parameterValue != DBNull.Value
                        &&
                        parameterValue != null
                    )
                {
                    if (parameterValue is DataTable dataTable)
                    {
                        if
                            (
                                jValue != null
                                &&
                                jValue.Type != JTokenType.Null
                                &&
                                jValue.Type != JTokenType.Undefined
                                &&
                                jValue.Type != JTokenType.None
                            )
                        {
                            if (jValue is JArray jArray)
                            {
                                var columns = dataTable.Columns;
                                var rows = dataTable.Rows;
                                foreach (var j in jArray)
                                {
                                    var row = dataTable.NewRow();
                                    var i = 0;
                                    foreach (DataColumn column in columns)
                                    {
                                        var columnName = column.ColumnName;
                                        if (i == 0 && j is JValue)
                                        {
                                            object jv = (JValue) j;
                                            row[columnName] = (jv ?? DBNull.Value);
                                            break;
                                        }
                                        else
                                        {
                                            if (j is JObject jo)
                                            {
                                                if 
                                                    (
                                                        jo
                                                            .TryGetValue
                                                                (
                                                                    columnName
                                                                    , StringComparison
                                                                            .OrdinalIgnoreCase
                                                                    , out var jToken
                                                                )
                                                    )
                                                {
                                                    var jv = jToken
                                                                .GetPrimtiveTypeJValueAsObject
                                                                    (
                                                                        column.DataType
                                                                    );
                                                    row[columnName] = (jv ?? DBNull.Value);
                                                }
                                            }
                                        }
                                        i++;
                                    }
                                    rows.Add(row);
                                }
                                @return = dataTable;
                            }
                        }
                    }
                }
            }
            else if
                (
                    jValue == null
                    ||
                    jValue.Type == JTokenType.Null
                    ||
                    jValue.Type == JTokenType.Undefined
                    ||
                    jValue.Type == JTokenType.None
                )
            {
                @return = DBNull.Value;
            }
            else
            {
                var jValueText = jValue.ToString();
                if
                    (
                        target.SqlDbType == SqlDbType.VarChar
                        ||
                        target.SqlDbType == SqlDbType.NVarChar
                        ||
                        target.SqlDbType == SqlDbType.Char
                        ||
                        target.SqlDbType == SqlDbType.NChar
                        ||
                        target.SqlDbType == SqlDbType.Text
                        ||
                        target.SqlDbType == SqlDbType.NText
                    )
                {
                    @return = jValueText;
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.DateTime
                        ||
                        target.SqlDbType == SqlDbType.DateTime2
                        ||
                        target.SqlDbType == SqlDbType.SmallDateTime
                        ||
                        target.SqlDbType == SqlDbType.Date
                        ||
                        target.SqlDbType == SqlDbType.DateTime
                    )
                {
                    if
                        (
                            DateTime
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.DateTimeOffset
                    )
                {
                    if
                        (
                            DateTimeOffset
                                    .TryParse
                                        (
                                            jValueText
                                            , out var @value
                                        )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.Bit
                    )
                {
                    if
                        (
                            bool
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.Decimal
                    )
                {
                    if
                        (
                            decimal
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.Float
                    )
                {
                    if
                        (
                            float
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.Real
                    )
                {
                    if
                        (
                            double
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.UniqueIdentifier
                    )
                {
                    if
                        (
                            Guid
                                .TryParse
                                    (
                                        jValueText
                                        , out Guid @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.BigInt
                    )
                {
                    if
                        (
                            long
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.Int
                    )
                {
                    if
                        (
                            int
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.SmallInt
                    )
                {
                    if
                        (
                            short
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.TinyInt
                    )
                {
                    if
                        (
                            short
                                .TryParse
                                    (
                                        jValueText
                                        , out var @value
                                    )
                        )
                    {
                        @return = @value;
                    }
                }
            }
            return @return;
        }
    }
}
