namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;

    public static partial class JsonHelper
    {
        public static IEnumerable<JValue> GetAllJValues(this JToken target)
        {
            if (target is JValue jValue)
            {
                yield return jValue;
            }
            else if (target is JArray jArray)
            {
                foreach (var result in GetAllJValuesFromJArray(jArray))
                {
                    yield return result;
                }
            }
            else if (target is JProperty jProperty)
            {
                foreach (var result in GetAllJValuesFromJProperty(jProperty))
                {
                    yield return result;
                }
            }
            else if (target is JObject jObject)
            {
                foreach (var result in GetAllValuesFromJObject(jObject))
                {
                    yield return result;
                }
            }
        }

        #region Private helpers

        public static IEnumerable<JValue> GetAllJValuesFromJArray(this JArray target)
        {
            for (var i = 0; i < target.Count; i++)
            {
                foreach (var result in GetAllJValues(target[i]))
                {
                    yield return result;
                }
            }
        }

        public static IEnumerable<JValue> GetAllJValuesFromJProperty(this JProperty target)
        {
            foreach (var result in GetAllJValues(target.Value))
            {
                yield return result;
            }
        }

        public static IEnumerable<JValue> GetAllValuesFromJObject(this JObject target)
        {
            foreach (var jToken in target.Children())
            {
                foreach (var result in GetAllJValues(jToken))
                {
                    yield return result;
                }
            }
        }

        #endregion
        public static JToken GetDescendantByPathKeys
                        (
                            this JToken target
                            , params string[] pathKeys
                        )
        {
            return
                target
                    .GetDescendantByPathKeys
                        (
                            true
                            , pathKeys
                        );
        }
        public static JToken GetDescendantByPathKeys
                                (
                                    this JToken target
                                    , bool ignoreCase = true
                                    , params string[] pathKeys
                                )
        {
            JToken jToken = target;
            foreach (var key in pathKeys)
            {
                if (key.IsNullOrEmptyOrWhiteSpace())
                {
                    break;
                }
                if (jToken is JArray)
                {
                    var b = int.TryParse(key, out var i);
                    if (b)
                    {
                        var ja = ((JArray)jToken);
                        if (i >= 0 && i < ja.Count)
                        {
                            jToken = ja[i];
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else if (jToken is JObject)
                {
                    if (ignoreCase)
                    {
                        var b = ((JObject)jToken)
                                        .TryGetValue
                                            (
                                                key
                                                , StringComparison
                                                        .OrdinalIgnoreCase
                                                , out var j
                                            );
                        if (b)
                        {
                            jToken = j;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        jToken = jToken[key];
                    }
                }
                else
                {
                    break;
                }
            }
            return jToken;
        }
        public static bool TryGetNullableValue<T>
                            (
                                this JToken target
                                , ref T jTokenValue
                            )
                        where T : struct
        {
            var r = false;
            Nullable<T> output = null;
            //jTokenValue = default(T);
            //jTokenValue = jTokenValue;
            if (target != null)
            {
                output = target.Value<Nullable<T>>();
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
                                this JToken target
                                , ref T jTokenValue
                            )
        {
            var r = false;
            //jTokenValue = default(T);
            //jTokenValue = jTokenValue;
            if (target != null)
            {
                jTokenValue = target.Value<T>();
                if (jTokenValue != null)
                {
                    r = true;
                }
            }
            return r;
        }
    }
}
