#if !XAMARIN
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using Npgsql;
    using NpgsqlTypes;
    using System;
    public static partial class DbParameterHelper
    {
        public static object SetParameterValue
                                    (
                                        this NpgsqlParameter target
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
                    target.NpgsqlDbType == NpgsqlDbType.Varchar
                    ||
                    target.NpgsqlDbType == NpgsqlDbType.Text
                    ||
                    target.NpgsqlDbType == NpgsqlDbType.Char
                )
                {
                    r = jValueText;
                }
                else if
                    (
                        target.NpgsqlDbType == NpgsqlDbType.Date
                        ||
                        target.NpgsqlDbType == NpgsqlDbType.Time
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
                        target.NpgsqlDbType == NpgsqlDbType.Bit
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
                        target.NpgsqlDbType == NpgsqlDbType.Double
                        ||
                        target.NpgsqlDbType == NpgsqlDbType.Real
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
                        target.NpgsqlDbType == NpgsqlDbType.Uuid
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
                        target.NpgsqlDbType == NpgsqlDbType.Bigint
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
                        target.NpgsqlDbType == NpgsqlDbType.Numeric
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
            }
            return r;
        }
    }
}
#endif