namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using IBM.Data.DB2.Core;

    public static partial class MySqlDbParameterHelper
    {
        public static object SetGetValueAsObject
                                    (
                                        this DB2Parameter target
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
                        target.DB2Type == DB2Type.LongVarChar
                        ||
                        target.DB2Type == DB2Type.VarChar
                        ||
                        target.DB2Type == DB2Type.NVarChar
                        ||
                        target.DB2Type == DB2Type.Char
                        ||
                        target.DB2Type == DB2Type.NChar
                    )
                {
                    r = jValueText;
                }
                else if
                    (
                        target.DB2Type == DB2Type.Date
                        ||
                        target.DB2Type == DB2Type.DateTime
                        ||
                        target.DB2Type == DB2Type.Time
                        ||
                        target.DB2Type == DB2Type.Timestamp
                        ||
                        target.DB2Type == DB2Type.TimeStampWithTimeZone
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
                        target.DB2Type == DB2Type.Boolean
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
                        target.DB2Type == DB2Type.Decimal
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
                        target.DB2Type == DB2Type.Double
                        ||
                        target.DB2Type == DB2Type.Real
                        ||
                        target.DB2Type == DB2Type.Real370
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
                        target.DB2Type == DB2Type.Float
                        ||
                        target.DB2Type == DB2Type.SmallFloat
                        ||
                        target.DB2Type == DB2Type.DecimalFloat
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
                        target.DB2Type == DB2Type.Byte
                    )
                {
                    var b = Guid
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
                        target.DB2Type == DB2Type.BigInt
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
                        target.DB2Type == DB2Type.Integer
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
                        target.DB2Type == DB2Type.SmallInt
                        ||
                        target.DB2Type == DB2Type.Int8
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
