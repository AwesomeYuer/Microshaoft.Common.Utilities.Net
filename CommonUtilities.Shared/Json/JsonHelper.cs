namespace Microshaoft
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Collections.Generic;


    public class JObjectComparer : IEqualityComparer<JObject>
    {
        public string[] ComparePropertiesNames;

        public bool Equals(JObject x, JObject y)
        {
            var r = false;
            foreach (var s in ComparePropertiesNames)
            {
                var xJTokenType = x[s].Type;
                var yJTokenType = y[s].Type;
                if (xJTokenType == yJTokenType)
                {
                    if (xJTokenType == JTokenType.String)
                    {
                        r = x[s].Value<string>() == y[s].Value<string>();
                    }
                    else if (xJTokenType == JTokenType.Integer)
                    {
                        r = x[s].Value<int>() == y[s].Value<int>();
                    }
                    else if (xJTokenType == JTokenType.Boolean)
                    {
                        r = x[s].Value<bool>() == y[s].Value<bool>();
                    }
                    else if (xJTokenType == JTokenType.Date)
                    {
                        r = x[s].Value<DateTime>() == y[s].Value<DateTime>();
                    }
                    else if (xJTokenType == JTokenType.Float)
                    {
                        r = x[s].Value<float>() == y[s].Value<float>();
                    }
                    else if (xJTokenType == JTokenType.Guid)
                    {
                        r = x[s].Value<Guid>() == y[s].Value<Guid>();
                    }
                    else if (xJTokenType == JTokenType.Guid)
                    {
                        r = x[s].Value<Guid>() == y[s].Value<Guid>();
                    }
                    else if (xJTokenType == JTokenType.TimeSpan)
                    {
                        r = x[s].Value<TimeSpan>() == y[s].Value<TimeSpan>();
                    }
                    //else if (xJTokenType == JTokenType.)
                    //{
                    //    r = x[s].Value<Guid>() == y[s].Value<Guid>();
                    //}
                }
                if (!r)
                {
                    break;
                }
            }
            return r;
        }

        public int GetHashCode(JObject obj)
        {
            return 0;
        }
    }

    public static class JsonHelper
    {
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
                            , (isJArray, jsonPath, valueObject, valueType, reader) =>
                            {
                                var vs = valueObject as string;
                                if (vs != null)
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