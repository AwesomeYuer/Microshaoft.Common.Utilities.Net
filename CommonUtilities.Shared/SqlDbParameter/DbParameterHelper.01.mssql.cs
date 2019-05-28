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
            object r = null;
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
                                            object jv = (JValue)j;
                                            row[columnName] = (jv == null ? DBNull.Value : jv);
                                            break;
                                        }
                                        else
                                        {
                                            var jo = j as JObject;
                                            if (jo != null)
                                            {
                                                var b = jo
                                                            .TryGetValue
                                                                (
                                                                    columnName
                                                                    , StringComparison
                                                                            .OrdinalIgnoreCase
                                                                    , out var jToken
                                                                );
                                                if (b)
                                                {
                                                    var jv = jToken
                                                                .GetPrimtiveTypeJValueAsObject
                                                                    (
                                                                        column.DataType
                                                                    );
                                                    row[columnName] = (jv == null ? DBNull.Value : jv);
                                                }
                                            }
                                        }
                                        i++;
                                    }
                                    rows.Add(row);
                                }
                                r = dataTable;
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
                r = DBNull.Value;
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
                    r = jValueText;
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
                    var b = DateTime
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.DateTimeOffset
                    )
                {
                    var b = DateTimeOffset
                                    .TryParse
                                        (
                                            jValueText
                                            , out var rr
                                        );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.Bit
                    )
                {
                    var b = bool
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.Decimal
                    )
                {

                    var b = decimal
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.Float
                    )
                {
                    var b = float
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.Real
                    )
                {
                    var b = double
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.UniqueIdentifier
                    )
                {
                    var b = Guid
                                .TryParse
                                    (
                                        jValueText
                                        , out Guid rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.BigInt
                    )
                {
                    var b = long
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.Int
                    )
                {
                    var b = int
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.SmallInt
                    )
                {
                    var b = short
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.SqlDbType == SqlDbType.TinyInt
                    )
                {
                    var b = short
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    );
                    if (b)
                    {
                        r = rr;
                    }
                }
            }
            return r;
        }
    }
}
