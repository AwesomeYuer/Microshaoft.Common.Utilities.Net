namespace Microshaoft
{
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json.Linq;
    using System;
    public static partial class MySqlDbParameterHelper
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
                    if 
                        (
                            DateTime
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    )
                        )
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.MySqlDbType == MySqlDbType.Bit
                    )
                {
                    if
                        (
                            bool
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    )
                        )
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.MySqlDbType == MySqlDbType.Decimal
                    )
                {
                    if
                        (
                            decimal
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    )
                        )
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.MySqlDbType == MySqlDbType.Float
                    )
                {
                    if
                        (
                            float
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    )
                        )
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.MySqlDbType == MySqlDbType.Guid
                    )
                {
                    if
                        (
                            Guid
                                .TryParse
                                    (
                                        jValueText
                                        , out Guid rr
                                    )
                        )
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.MySqlDbType == MySqlDbType.UInt16
                    )
                {
                    if
                        (
                            ushort
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    )
                        )
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
                    if
                        (
                            uint
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    )
                        )
                    {
                        r = rr;
                    }
                }
                else if
                    (
                        target.MySqlDbType == MySqlDbType.UInt64
                    )
                {
                    if
                        (
                            ulong
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    )
                        )
                    {
                        r = rr;
                    }
                }
                else if
                   (
                       target.MySqlDbType == MySqlDbType.Int16
                   )
                {
                    if
                        (
                            short
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    )
                        )
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
                    if
                        (
                            int
                                .TryParse
                                    (
                                        jValueText
                                        , out var rr
                                    )
                        )
                    {
                        r = rr;
                    }
                }
                else if
                   (
                        target.MySqlDbType == MySqlDbType.Int64
                   )
                {
                    if
                        (
                            long
                                .TryParse
                                    (
                                        jValueText
                                        , out long rr
                                    )
                        )
                    {
                        r = rr;
                    }
                }
            }
            return r;
        }
    }
}
