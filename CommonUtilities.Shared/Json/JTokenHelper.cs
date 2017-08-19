
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    public static class JTokenHelper
    {
        public static bool TryGetNullableValue<T>
                            (
                                JToken jToken
                                , ref T jTokenValue
                            )
                        where T : struct
        {
            var r = false;
            Nullable<T> output = null;
            //jTokenValue = default(T);
            //jTokenValue = jTokenValue;
            if (jToken != null)
            {
                output = jToken.Value<Nullable<T>>();
                if (output.HasValue)
                {
                    jTokenValue = output.Value;
                    r = true;
                }
            }
            return r;
        }
        public static bool TryGetNonNullValue<T>
                            (
                                JToken jToken
                                , ref T jTokenValue
                            )
        {
            var r = false;
            //jTokenValue = default(T);
            //jTokenValue = jTokenValue;
            if (jToken != null)
            {
                jTokenValue = jToken.Value<T>();
                if (jTokenValue != null)
                {
                    r = true;
                }
            }
            return r;
        }
    }
}
