namespace Microshaoft
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Collections.Generic;

    public class JObjectComparer : IEqualityComparer<JToken>
    {
        public string[] CompareJTokensPaths;

        public bool Equals(JToken x, JToken y)
        {
            var r = false;
            foreach (var path in CompareJTokensPaths)
            {
                var xJTokenType = x.SelectToken(path).Type;
                var yJTokenType = y.SelectToken(path).Type;
                if (xJTokenType == yJTokenType)
                {
                    if (xJTokenType == JTokenType.String)
                    {
                        r = x.SelectToken(path).Value<string>() == y.SelectToken(path).Value<string>();
                    }
                    else if (xJTokenType == JTokenType.Integer)
                    {
                        r = x.SelectToken(path).Value<int>() == y.SelectToken(path).Value<int>();
                    }
                    else if (xJTokenType == JTokenType.Boolean)
                    {
                        r = x.SelectToken(path).Value<bool>() == y.SelectToken(path).Value<bool>();
                    }
                    else if (xJTokenType == JTokenType.Date)
                    {
                        r = x.SelectToken(path).Value<DateTime>() == y.SelectToken(path).Value<DateTime>();
                    }
                    else if (xJTokenType == JTokenType.Float)
                    {
                        r = x.SelectToken(path).Value<float>() == y.SelectToken(path).Value<float>();
                    }
                    else if (xJTokenType == JTokenType.Guid)
                    {
                        r = x.SelectToken(path).Value<Guid>() == y.SelectToken(path).Value<Guid>();
                    }
                    else if (xJTokenType == JTokenType.Guid)
                    {
                        r = x.SelectToken(path).Value<Guid>() == y.SelectToken(path).Value<Guid>();
                    }
                    else if (xJTokenType == JTokenType.TimeSpan)
                    {
                        r = x.SelectToken(path).Value<TimeSpan>() == y.SelectToken(path).Value<TimeSpan>();
                    }
                    //else if (xJTokenType == JTokenType.)
                    //{
                    //    r = x.SelectToken(path).Value<Guid>() == y.SelectToken(path).Value<Guid>();
                    //}
                }
                if (!r)
                {
                    break;
                }
            }
            return r;
        }
        public int GetHashCode(JToken obj)
        {
            return 0;
        }
    }

    public static partial class JsonHelper
    {
        public static JTokenType TryParseJson
                                    (
                                        this string target
                                        , out JToken jToken
                                    )
        {
            var r = JTokenType.None;
            jToken = null;
            try
            {
                jToken = JToken.Parse(target);
                if (jToken is JArray)
                {
                    r = JTokenType.Array;
                }
                else if (jToken is JObject)
                {
                    r = JTokenType.Object;
                }
            }
            catch
            {

            }
            return r;
        }

        public static bool IsJson
                            (
                                this string target
                                , out JToken jToken
                                , bool validate = false
                            )
        {
            jToken = null;
            char c = target.FirstNonWhitespaceCharacter();
            //IL_0018: Unknown result type (might be due to invalid IL or missing references)
            bool r = (c == '{' || c == '[');
            if (r && validate)
            {
                try
                {
                    jToken = JToken.Parse(target);
                    r = true;
                }
                catch
                {
                    r = false;
                }
            }
            return r;
        }

        public static bool IsJArray(this string target, bool validate = false)
        {
            char c = target.FirstNonWhitespaceCharacter();
            //IL_0018: Unknown result type (might be due to invalid IL or missing references)
            bool r = (c == '[');
            if (r && validate)
            {
                try
                {
                    r = 
                        (
                            TryParseJson(target, out _)
                            ==
                            JTokenType.Array
                        );
                }
                catch
                {
                    r = false;
                }
            }
            return r;
        }

        public static bool TryParseJArray
                                (
                                    this string target
                                    , out JArray jArray
                                )
        {
            //IL_0018: Unknown result type (might be due to invalid IL or missing references)
            var r = false;
            jArray = null;
            if (r)
            {
                try
                {
                    r = 
                        (
                            TryParseJson
                                (
                                    target
                                    , out JToken jToken
                                ) 
                            ==
                            JTokenType.Array
                        );
                    if (r)
                    {
                        jArray = jToken as JArray;
                    }
                }
                catch
                {
                    r = false;
                }
            }
            return r;
        }

        public static bool TryParseJObject
                                (
                                    this string target
                                    , out JObject jObject
                                )
        {
            //IL_0018: Unknown result type (might be due to invalid IL or missing references)
            var r = false;
            jObject = null;
            if (r)
            {
                try
                {
                    r = (TryParseJson(target, out JToken jToken) == JTokenType.Object);
                    if (r)
                    {
                        jObject = jToken as JObject;
                    }
                }
                catch
                {
                    r = false;
                }
            }
            return r;
        }

        public static bool IsJObject(this string target, bool validate = false)
        {
            char c = StringHelper.FirstNonWhitespaceCharacter(target);
            //IL_0018: Unknown result type (might be due to invalid IL or missing references)
            var r = (c == '{');
            if (r && validate)
            {
                try
                {
                    r = (TryParseJson(target, out _) == JTokenType.Object);
                }
                catch
                {
                    r = false;
                }
            }
            return r;
        }

        public static object GetPrimtiveTypeJValueAsObject
                                    (
                                        this JToken target
                                        , Type underlyingType
                                        //, object nullValue = DBNull.Value
                                    )
        {
            object r = null;
            if 
                (
                    target.Type == JTokenType.Null 
                    ||
                    target.Type == JTokenType.Undefined
                    ||
                    target.Type == JTokenType.None
                )
            {
                r = null;
            }
            // with ""
            else if (underlyingType == typeof(string))
            {
                string jValueString;
                if (target.Type != JTokenType.String)
                {
                    jValueString = target.ToString();
                }
                else
                {
                    jValueString = target.Value<string>();
                }
                r = jValueString;
            }
            else if (underlyingType == typeof(Guid))
            {
                var jValueString = target.Value<string>();
                r = Guid.Parse(jValueString);
            }
            else if (underlyingType == typeof(DateTime))
            {
                var jValueString = target.Value<string>();
                r = DateTime.Parse(jValueString);
            }
            //===============================================
            // without ""
            else if (underlyingType == typeof(bool))
            {
                r = target.Value<bool>();
            }
            else if (underlyingType == typeof(short))
            {
                r = target.Value<short>();
            }
            else if (underlyingType == typeof(ushort))
            {
                r = target.Value<ushort>();
            }
            else if (underlyingType == typeof(int))
            {
                r = target.Value<int>();
            }
            else if (underlyingType == typeof(uint))
            {
                r = target.Value<uint>();
            }
            else if (underlyingType == typeof(long))
            {
                r = target.Value<long>();
            }
            else if (underlyingType == typeof(ulong))
            {
                r = target.Value<ulong>();
            }
            else if (underlyingType == typeof(double))
            {
                r = target.Value<double>();
            }
            else if (underlyingType == typeof(float))
            {
                r = target.Value<float>();
            }
            else if (underlyingType == typeof(decimal))
            {
                r = target.Value<decimal>();
            }
            return r;
        }
        public static JTokenType GetJTokenType(this Type target)
        {
            JTokenType r = JTokenType.None;
            if
                (
                    typeof(bool) == target
                )
            {
                r = JTokenType.Boolean;
            }
            else if
                (
                    typeof(byte[]) == target
                )
            {
                r = JTokenType.Bytes;
            }
            else if
                (
                    typeof(DateTime) == target
                )
            {
                r = JTokenType.Date;
            }
            else if
                (
                    typeof(float) == target
                    ||
                    typeof(double) == target
                    ||
                    typeof(decimal) == target
                    ||
                    typeof(Single) == target
                )
            {
                r = JTokenType.Float;
            }
            else if
                (
                    typeof(Guid) == target
                )
            {
                r = JTokenType.Guid;
            }
            else if
                (
                    typeof(int) == target
                    ||
                    typeof(long) == target
                    ||
                    typeof(short) == target
                    ||
                    typeof(uint) == target
                    ||
                    typeof(ushort) == target
                    ||
                    typeof(byte) == target
                )
            {
                r = JTokenType.Integer;
            }
            else if
                (
                    typeof(TimeSpan) == target
                )
            {
                r = JTokenType.TimeSpan;
            }
            else
            {
                r = JTokenType.String;
            }
            return r;
        }
        public static JToken MergeJsonTemplateToJToken
                        (
                            string jsonTemplate
                            , string jsonData
                            , string jsonTemplatePathPrefix = "@"
                        )
        {
            var jTokenTemplate = JToken.Parse(jsonTemplate);
            var jTokenData = JToken.Parse(jsonData);
            JsonReaderHelper
                    .ReadAllPaths
                        (
                            jsonTemplate
                            , 
                                (
                                    isJArray
                                    , jsonPath
                                    , valueObject
                                    , valueType
                                    , reader
                                ) =>
                            {
                                if (valueObject is string vs)
                                {
                                    vs = vs.Trim();
                                    if (vs.StartsWith(jsonTemplatePathPrefix))
                                    {
                                        var replacedSelectToken = jTokenTemplate.SelectToken(jsonPath);
                                        var trimChars = jsonTemplatePathPrefix.ToCharArray();
                                        vs = vs.TrimStart(trimChars);
                                        var replacementSelectToken = jTokenData.SelectToken(vs);
                                        replacedSelectToken.Replace(replacementSelectToken);
                                    }
                                }
                                return false;
                            }
                        );
            return jTokenTemplate;
        }
        public static string MergeJsonTemplate
                (
                    string jsonTemplate
                    , string jsonData
                    , string jsonTemplatePathPrefix = "@"
                )
        {
            
            return
                    MergeJsonTemplateToJToken
                                (
                                    jsonTemplate
                                    , jsonData
                                    , jsonTemplatePathPrefix
                                )
                                .ToString();
        }

        public static string XmlToJson
                                (
                                    string xml
                                    , Newtonsoft
                                            .Json
                                            .Formatting formatting
                                                            = Newtonsoft
                                                                    .Json
                                                                    .Formatting
                                                                    .Indented
                                    , bool needKeyQuote = false
                                )
        {
            XNode xElement;
            xElement = XElement.Parse(xml).Elements().First();
            string json = string.Empty;
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    jsonTextWriter.Formatting = formatting;
                    jsonTextWriter.QuoteName = needKeyQuote;
                    var jsonSerializer = new JsonSerializer();
                    jsonSerializer.Serialize(jsonTextWriter, xElement);
                    json = stringWriter.ToString();
                }
            }
            return json;
        }
        public static string JsonToXml
                        (
                            string json
                            , bool needRoot = false
                            , string defaultDeserializeRootElementName = "root"
                        )
        {
            if (needRoot)
            {
                json = string.Format
                                (
                                    @"{{ {1}{0}{2} }}"
                                    , " : "
                                    , defaultDeserializeRootElementName
                                    , json
                                );
            }
            //XmlDocument xmlDocument = JsonConvert.DeserializeXmlNode(json, defaultDeserializeRootElementName);
            var xDocument = JsonConvert
                                    .DeserializeXNode
                                        (
                                            json
                                            , defaultDeserializeRootElementName
                                        );
            var xml = xDocument
                            .Elements()
                            .First()
                            .ToString();
            return xml;
        }
        public static T DeserializeByJTokenPath<T>
            (
                string json
                , string jTokenPath = null //string.Empty
            )
        {
            var jObject = JObject.Parse(json);
            var jsonSerializer = new JsonSerializer();
            if (string.IsNullOrEmpty(jTokenPath))
            {
                jTokenPath = string.Empty;
            }
            var jToken = jObject.SelectToken(jTokenPath);
            using (var jsonReader = jToken.CreateReader())
            {
                return
                    jsonSerializer
                        .Deserialize<T>(jsonReader);
            }
        }
        public static string Serialize
                                (
                                    object target
                                    , bool formattingIndented = false
                                    , bool keyQuoteName = false
                                )
        {
            string json = string.Empty;
            using (StringWriter stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    jsonTextWriter.QuoteName = keyQuoteName;
                    jsonTextWriter.Formatting = (formattingIndented ? Formatting.Indented : Formatting.None);
                    var jsonSerializer = new JsonSerializer();
                    jsonSerializer.Serialize(jsonTextWriter, target);
                    json = stringWriter.ToString();
                }
            }
            return json;
        }
        public static void ReadJsonPathsValuesAsStrings
                            (
                                string json
                                , string[] jsonPaths
                                , Func<string, string, bool> onReadedOncePathStringValueProcesssFunc = null
                            )
        {
            using (var stringReader = new StringReader(json))
            {
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    bool breakAndReturn = false;
                    while
                        (
                            jsonReader.Read()
                            &&
                            !breakAndReturn
                        )
                    {
                        foreach (var x in jsonPaths)
                        {
                            if (x == jsonReader.Path)
                            {
                                if (onReadedOncePathStringValueProcesssFunc != null)
                                {
                                    var s = jsonReader.ReadAsString();
                                    breakAndReturn
                                            = onReadedOncePathStringValueProcesssFunc
                                                    (
                                                        x
                                                        , s
                                                    );
                                    if (breakAndReturn)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public static IEnumerable<TElement>
                            DeserializeToFromDictionary<TKey, TValue, TElement>
                                        (
                                            string json
                                            , Func<TKey, TValue, TElement> OnOneElementProcessFunc
                                        )
        {
            //IEnumerable<TElement> r = default(IEnumerable<TElement>);
            return
                    DeserializeByJTokenPath<Dictionary<TKey, TValue>>(json)
                        .Select
                            (
                                (x) =>
                                {
                                    var rr = OnOneElementProcessFunc(x.Key, x.Value);
                                    return rr;
                                }
                            );
            //return r;
        }
    }
}