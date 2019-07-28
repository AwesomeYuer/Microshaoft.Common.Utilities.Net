#if !XAMARIN
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using Npgsql;
    using NpgsqlTypes;
    using System;
    public static partial class NpgsqlDbParameterHelper
    {
        public static object SetGetValueAsObject
                                    (
                                        this NpgsqlParameter target
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
                    target.NpgsqlDbType == NpgsqlDbType.Varchar
                    ||
                    target.NpgsqlDbType == NpgsqlDbType.Text
                    ||
                    target.NpgsqlDbType == NpgsqlDbType.Char
                )
                {
                    @return = jValueText;
                }
                else if
                    (
                        target.NpgsqlDbType == NpgsqlDbType.Date
                        ||
                        target.NpgsqlDbType == NpgsqlDbType.Time
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
                        target.NpgsqlDbType == NpgsqlDbType.Bit
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
                        target.NpgsqlDbType == NpgsqlDbType.Double
                        ||
                        target.NpgsqlDbType == NpgsqlDbType.Real
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
                        target.NpgsqlDbType == NpgsqlDbType.Uuid
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
                        target.NpgsqlDbType == NpgsqlDbType.Integer
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
                        target.NpgsqlDbType == NpgsqlDbType.Bigint
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
                        target.NpgsqlDbType == NpgsqlDbType.Numeric
                   )
                {
                    var b = decimal
                               .TryParse
                                   (
                                       jValueText
                                       , out var @value
                                   );
                    if (b)
                    {
                        @return = @value;
                    }
                }
            }
            return @return;
        }
    }
}
#endif