namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class JsonHelper
    {
        public static JToken MapToNew
                            (
                                this JToken source
                                , params
                                    (
                                        string TargetJPath
                                        , string SourceJPath
                                    )[]
                                        mappings

                            )
        {
            return
                MapToNew
                    (
                        source
                        , mappings
                        , false
                    );
        }
        public static JToken MapToNew
                            (
                                this JToken source
                                , bool orderedTargetPaths = false
                                , params
                                    (
                                        string TargetJPath
                                        , string SourceJPath
                                    )[]
                                        mappings
                                
                            )
        {
            return
                MapToNew
                    (
                        source
                        ,
                            //(
                            //    IEnumerable
                            //        <
                            //            (
                            //                string TargetJPath
                            //                , string SourceJPath
                            //            )
                            //        >
                            //)
                                mappings
                         , orderedTargetPaths
                    );
        }
        //TargetJPath such as: SomeField[1] unsupported JArray 
        public static JToken MapToNew
                            (
                                this JToken source
                                , IEnumerable
                                    <
                                        (
                                            string TargetJPath
                                            , string SourceJPath
                                        )
                                    >
                                        mappings
                                , bool orderedTargetPaths = false
                            )
        {
            JToken r = null;
            var c = mappings;
            if (orderedTargetPaths)
            {
                c = c
                        .OrderBy
                                (
                                    (x) =>
                                    {
                                        return
                                            x.TargetJPath;
                                    }
                                );
            }
            foreach (var (TargetJPath, SourceJPath) in c)
            {
                var jToken = source
                                .SelectToken(SourceJPath);
                if (TargetJPath != "$")
                {
                    if (r == null)
                    {
                        r = new JObject();
                    }
                    var ss = TargetJPath
                                    .Split
                                        (
                                            '.'
                                            //, StringSplitOptions
                                            //        .RemoveEmptyEntries
                                        );
                    var j = r;
                    var l = ss.Length;
                    for (var i = 0; i < l; i++)
                    {
                        var s = ss[i];
                        if (i < l - 1)
                        {
                            if (j[s] == null)
                            {
                                j[s] = new JObject();
                            }
                            j = j[s];
                        }
                        else
                        {
                            j[s] = jToken;
                        }
                    }
                }
                else //if (x.Key == "$")
                {
                    r = jToken;
                    break;
                }
            }
            return r;
        }
        public static IEnumerable<JValue> GetAllJValues
                        (
                            this JToken target
                        )
        {
            if (target is JValue jValue)
            {
                yield
                    return
                        jValue;
            }
            else if (target is JArray jArray)
            {
                var c = GetAllJValuesFromJArray(jArray);
                foreach (var result in c)
                {
                    yield
                        return
                            result;
                }
            }
            else if (target is JProperty jProperty)
            {
                var c = GetAllJValuesFromJProperty(jProperty);
                foreach (var result in c)
                {
                    yield return result;
                }
            }
            else if (target is JObject jObject)
            {
                var c = GetAllValuesFromJObject(jObject);
                foreach (var result in c)
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
                var c = GetAllJValues(target[i]);
                foreach (var result in c)
                {
                    yield return result;
                }
            }
        }

        public static IEnumerable<JValue> GetAllJValuesFromJProperty(this JProperty target)
        {
            var c = GetAllJValues(target.Value);
            foreach (var result in c)
            {
                yield return result;
            }
        }

        public static IEnumerable<JValue> GetAllValuesFromJObject(this JObject target)
        {
            var c = target.Children();
            foreach (var jToken in c)
            {
                var cc = GetAllJValues(jToken);
                foreach (var result in cc)
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
                    //jToken = null;
                    break;
                }
                if (jToken is JArray jArray)
                {
                    if (int.TryParse(key, out var i))
                    {
                        if (i >= 0 && i < jArray.Count)
                        {
                            jToken = jArray[i];
                        }
                        else
                        {
                            jToken = null;
                            break;
                        }
                    }
                    else
                    {
                        jToken = null;
                        break;
                    }
                }
                else if (jToken is JObject jObject)
                {
                    if (ignoreCase)
                    {
                        if 
                            (
                                jObject
                                    .TryGetValue
                                        (
                                            key
                                            , StringComparison
                                                    .OrdinalIgnoreCase
                                            , out var j
                                        )
                            )
                        {
                            jToken = j;
                        }
                        else
                        {
                            jToken = null;
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
                    jToken = null;
                    break;
                }
            }
            return
                jToken;
        }
        public static bool TryGetNullableValue<T>
                            (
                                this JToken target
                                , ref T jTokenValue
                            )
                        where T : struct
        {
            var r = false;
            //jTokenValue = default(T);
            //jTokenValue = jTokenValue;
            if (target != null)
            {
                T? output = target.Value<T?>();
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
