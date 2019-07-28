namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using IBM.Data.DB2.Core;

    public static partial class DB2DbParameterHelper
    {
        public static object SetGetValueAsObject
                                    (
                                        this DB2Parameter target
                                        , JToken jValue 
                                    )
        {
            object @return = null;
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
                @return = DBNull.Value;
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
                    @return = jValueText;
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
                        target.DB2Type == DB2Type.Boolean
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
                        target.DB2Type == DB2Type.Decimal
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
                        target.DB2Type == DB2Type.Double
                        ||
                        target.DB2Type == DB2Type.Real
                        ||
                        target.DB2Type == DB2Type.Real370
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
                        target.DB2Type == DB2Type.Float
                        ||
                        target.DB2Type == DB2Type.SmallFloat
                        ||
                        target.DB2Type == DB2Type.DecimalFloat
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
                        target.DB2Type == DB2Type.Byte
                    )
                {
                    if
                        (
                            Guid
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
                        target.DB2Type == DB2Type.BigInt
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
                        target.DB2Type == DB2Type.Integer
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
                        target.DB2Type == DB2Type.SmallInt
                        ||
                        target.DB2Type == DB2Type.Int8
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
