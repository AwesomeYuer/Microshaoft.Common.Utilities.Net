namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    public static class JTokenHelper
    {
        public static JToken GetDescendantByPath(this JToken target, params string[] paths)
        {
            var jsonPath = string.Empty;
            foreach (var path in paths)
            {
                if (path.IsNullOrEmptyOrWhiteSpace())
                {
                    break;
                }
                int i = -1;
                if (int.TryParse(path, out i))
                {
                    //i--;
                    jsonPath += string.Format("[{0}]", i);
                }
                else
                {
                    if (!jsonPath.IsNullOrEmptyOrWhiteSpace())
                    {
                        jsonPath += ".";
                    }
                    jsonPath += path;
                }
                //result = GetChild(segment, result);
            }
            if (!jsonPath.IsNullOrEmptyOrWhiteSpace())
            {
                target = target.SelectToken(jsonPath);
            }

            return target;
        }

        public static JToken GetChildByPath(this JToken target, string key)
        {
            object oKey = key;
            int iKey = -1;
            if (int.TryParse(key, out iKey))
            {
                oKey = iKey;// - 1;
            }
            target = target[oKey];
            return target;
        }

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
