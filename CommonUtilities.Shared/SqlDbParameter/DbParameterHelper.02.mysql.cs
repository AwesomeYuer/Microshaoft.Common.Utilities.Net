namespace Microshaoft
{
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using System;
    public static partial class DbParameterHelper
    {
        public static object SetGetValueAsObject
                                    (
                                        this MySqlParameter target
                                        , JToken jValue
                                    )
        {
            object r = null;
            if
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
                        target.MySqlDbType == MySqlDbType.VarChar
                        ||
                        target.MySqlDbType == MySqlDbType.Text
                        ||
                        target.MySqlDbType == MySqlDbType.VarString
                    )
                {
                    r = jValueText;
                }
                else if
                    (
                        target.MySqlDbType == MySqlDbType.DateTime
                        ||
                        target.MySqlDbType == MySqlDbType.Date
                        ||
                        target.MySqlDbType == MySqlDbType.DateTime
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
                        target.MySqlDbType == MySqlDbType.Bit
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
                        target.MySqlDbType == MySqlDbType.Decimal
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
                        target.MySqlDbType == MySqlDbType.Float
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
                        target.MySqlDbType == MySqlDbType.Guid
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
                        target.MySqlDbType == MySqlDbType.UInt16
                    )
                {
                    var b = ushort
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
                        target.MySqlDbType == MySqlDbType.UInt24
                        ||
                        target.MySqlDbType == MySqlDbType.UInt32
                    )
                {
                    var b = uint
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
                        target.MySqlDbType == MySqlDbType.UInt64
                    )
                {
                    var b = ulong
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
                       target.MySqlDbType == MySqlDbType.Int16
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
                        target.MySqlDbType == MySqlDbType.Int24
                        ||
                        target.MySqlDbType == MySqlDbType.Int32
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
                        target.MySqlDbType == MySqlDbType.Int64
                   )
                {
                    var b = long
                                .TryParse
                                    (
                                        jValueText
                                        , out long rr
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
