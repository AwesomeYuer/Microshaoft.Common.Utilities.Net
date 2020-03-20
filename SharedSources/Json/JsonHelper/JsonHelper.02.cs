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
                                this JToken @this
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
                        @this
                        , mappings
                        , false
                    );
        }
        public static JToken MapToNew
                            (
                                this JToken @this
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
                        @this
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
                                this JToken @this
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
                                            x
                                                .TargetJPath;
                                    }
                                );
            }
            foreach (var (TargetJPath, SourceJPath) in c)
            {
                var jToken = @this
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
                            this JToken @this
                        )
        {
            if (@this is JValue jValue)
            {
                yield
                    return
                        jValue;
            }
            else if (@this is JArray jArray)
            {
                var c = GetAllJValuesFromJArray(jArray);
                foreach (var result in c)
                {
                    yield
                        return
                            result;
                }
            }
            else if (@this is JProperty jProperty)
            {
                var c = GetAllJValuesFromJProperty(jProperty);
                foreach (var result in c)
                {
                    yield return result;
                }
            }
            else if (@this is JObject jObject)
            {
                var c = GetAllValuesFromJObject(jObject);
                foreach (var result in c)
                {
                    yield return result;
                }
            }
        }

        #region Private helpers

        public static IEnumerable<JValue> GetAllJValuesFromJArray(this JArray @this)
        {
            int count = @this.Count;
            for (var i = 0; i < count; i++)
            {
                var c = GetAllJValues(@this[i]);
                foreach (var result in c)
                {
                    yield
                        return
                            result;
                }
            }
        }

        public static IEnumerable<JValue> GetAllJValuesFromJProperty(this JProperty @this)
        {
            var c = GetAllJValues(@this.Value);
            foreach (var result in c)
            {
                yield
                    return
                        result;
            }
        }

        public static IEnumerable<JValue> GetAllValuesFromJObject(this JObject @this)
        {
            var c = @this.Children();
            foreach (var jToken in c)
            {
                var cc = GetAllJValues(jToken);
                foreach (var result in cc)
                {
                    yield
                        return
                            result;
                }
            }
        }

        #endregion
        public static JToken GetDescendantByPathKeys
                        (
                            this JToken @this
                            , params string[] pathKeys
                        )
        {
            return
                @this
                    .GetDescendantByPathKeys
                        (
                            true
                            , pathKeys
                        );
        }
        public static JToken GetDescendantByPathKeys
                                (
                                    this JToken @this
                                    , bool ignoreCase = true
                                    , params string[] pathKeys
                                )
        {
            JToken jToken = @this;
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
                                this JToken @this
                                , ref T jTokenValue
                            )
                        where T : struct
        {
            var r = false;
            //jTokenValue = default(T);
            //jTokenValue = jTokenValue;
            if (@this != null)
            {
                T? output = @this.Value<T?>();
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
                                this JToken @this
                                , ref T jTokenValue
                            )
        {
            var r = false;
            //jTokenValue = default(T);
            //jTokenValue = jTokenValue;
            if (@this != null)
            {
                jTokenValue = @this.Value<T>();
                if (jTokenValue != null)
                {
                    r = true;
                }
            }
            return r;
        }
    }
}
