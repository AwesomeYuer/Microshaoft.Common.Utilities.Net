namespace Test
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System.Globalization;
    class Program123
    {
        static void Main114(string[] args)
        {
            string json = @"{ 'name10': 
'Admin',c:1111,d:[{a:'aaaa'}] ,e:[1,]}
[{ 'name9': 'Publisher' }]


[
{ 'name4': 'Admin' },{ 'name8': ['Admin'] }]{ 'name7': 
'Admin' }
[{ 'name3': ['Publisher','Publisher'] }]{ 'name5': 
'Admin' }
[{ 'name2': 'Publisher' }]{ 'name6': 
'Admin' }
[[{ 'name1': 'Publisher' }]]";

            JsonReader reader = new JsonTextReader(new StringReader(json));
            reader.ReadAllMultipleContents();
            //reader.re



            //foreach (var i in r)
            //{

            //    Console.WriteLine(i);
            //}
            Console.ReadLine();
        }
        static void Main2(string[] args)
        {

            string json = @"{ 'name10': 
'Admin' }
[{ 'name9': 'Publisher' }][
{ 'name4': 'Admin' },{ 'name8': ['Admin'] }]{ 'name7': 
'Admin' }
[{ 'name3': ['Publisher','Publisher'] }]{ 'name5': 
'Admin' }
[{ 'name2': 'Publisher' }]{ 'name6': 
'Admin' }
[{ 'name1': 'Publisher' }]";

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            var r = reader.ReadAllMultipleContentsAsEnumerable<JObject>().ToArray();
            reader = new JsonTextReader(new StringReader(json));
            r = reader
                    .ReadMultipleContentsAsEnumerable<JObject>(3)
                    .SelectMany
                        (
                            (x) =>
                            {
                                return x;
                            }
                        ).ToArray();
            Console.ReadLine();
        }

        static void Main1(string[] args)
        {

            string json = @"{ 'name': 
'Admin',c:1111111 }
[{ 'name': 'Publisher' }][
{ 'name': 'Admin' },{ 'name': 'Admin' }]{ 'name': 
'Admin' }
[{ 'name': 'Publisher' }]{ 'name': 
'Admin' }
[{ 'name': 'Publisher' }]{ 'name': 
'Admin' }
[{ 'name': 'Publisher' }]";

            IList<Role> roles = new List<Role>();

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            var r = reader.ReadAllMultipleContentsAsEnumerable<Role>().ToArray();

            reader = new JsonTextReader(new StringReader(json));
            r = reader
                    .ReadMultipleContentsAsEnumerable<Role>(3)
                    .SelectMany
                        (
                            (x) =>
                            {
                                return x;
                            }
                        ).ToArray();


            Console.ReadLine();
        }
    }

    public class Role
    {

        public string Name { get; set; }
    }





}



namespace Microshaoft
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System;
    using System.IO;
    using System.Globalization;

    public static class JsonReaderHelper
    {
        public static void ReadAllPaths
                    (
                        string json
                        , Func<bool, string, object, Type, JsonReader, bool> onReadPathOnceProcessFunc
                    )
        {
            using (JsonReader reader = new JsonTextReader(new StringReader(json)))
            {
                var isStarted = false;
                var isJArray = false;
                while (reader.Read())
                {
                    JsonToken tokenType = reader.TokenType;
                    if (!isStarted)
                    {
                        if (tokenType == JsonToken.StartArray)
                        {
                            isJArray = true;
                            isStarted = true;
                        }
                        else if (tokenType == JsonToken.StartArray)
                        {

                            isStarted = true;
                        }
                        else if (tokenType == JsonToken.StartConstructor)
                        {
                            isStarted = true;
                        }
                    }
                    if
                        (
                            tokenType != JsonToken.Comment
                            &&
                            tokenType != JsonToken.PropertyName
                        )
                    {
                        var jsonPath = reader.Path;
                        if (!string.IsNullOrEmpty(jsonPath))
                        {
                            var valueType = reader.ValueType;
                            var valueObject = reader.Value;
                            if (valueType != null)
                            {
                                var r = onReadPathOnceProcessFunc
                                                (
                                                    isJArray
                                                    , jsonPath
                                                    , valueObject
                                                    , valueType
                                                    , reader
                                                );
                                if (r)
                                {
                                    break;
                                }
                            }
                        }
                    }

                }
                reader.Close();
            }
        }
        public static void ReadAllMultipleContents
                                        (
                                            this JsonReader target
                                        )
        {
            if (!target.SupportMultipleContent)
            {
                target.SupportMultipleContent = true;
            }
            var serializer = new JsonSerializer();
            //serializer.CheckAdditionalContent
            while (target.Read())
            {
                Console.WriteLine(target.TokenType);
                var r = serializer.Deserialize(target);
                Console.WriteLine(r.GetType());
                Console.WriteLine(r.ToString());
            }
        }
        public static IEnumerable<JToken> ReadMultipleContents
                                                (
                                                    this JsonReader target
                                                )
        {
            if (!target.SupportMultipleContent)
            {
                target.SupportMultipleContent = true;
            }
            var serializer = new JsonSerializer();
            while (target.Read())
            {
                if (target.TokenType == JsonToken.StartObject)
                {
                    JToken entry = serializer.Deserialize<JToken>(target);
                    yield return entry;
                }
                else if (target.TokenType == JsonToken.StartArray)
                {
                    JArray entries = serializer.Deserialize<JArray>(target);
                    foreach (var entry in entries)
                    {
                        if (entry is JArray)
                        {
                            //Console.WriteLine();
                        }
                        yield return (JToken)entry;
                    }
                }
            }
        }
        public static IEnumerable<IEnumerable<T>> ReadMultipleContentsAsEnumerable<T>
                        (
                            this JsonReader target
                            , int pageSize = 10
                        )
        {
            if (!target.SupportMultipleContent)
            {
                target.SupportMultipleContent = true;
            }
            var serializer = new JsonSerializer();
            var list = new List<T>();
            var i = 0;
            while (target.Read())
            {
                if (target.TokenType == JsonToken.StartArray)
                {
                    var entries = serializer.Deserialize<T[]>(target);
                    foreach (var entry in entries)
                    {
                        if (i < pageSize)
                        {
                            i++;
                            list.Add(entry);
                        }
                        if (i >= pageSize)
                        {
                            yield return list;
                            list.Clear();
                            i = 0;
                        }
                    }
                }
                else
                {
                    var entry = serializer.Deserialize<T>(target);
                    if (i < pageSize)
                    {
                        i++;
                        list.Add(entry);
                    }
                    if (i >= pageSize)
                    {
                        yield return list;
                        list.Clear();
                        i = 0;
                    }
                }
            }
            if (i > 0)
            {
                yield return list;
                list.Clear();
                i = 0;
                list = null;
            }
        }

        public static IEnumerable<T> ReadAllMultipleContentsAsEnumerable<T>(this JsonReader target)
        {
            if (!target.SupportMultipleContent)
            {
                target.SupportMultipleContent = true;
            }
            var serializer = new JsonSerializer();
            while (target.Read())
            {
                if (target.TokenType == JsonToken.StartArray)
                {
                    var entries = serializer.Deserialize<T[]>(target);
                    foreach (var entry in entries)
                    {
                        yield return entry;
                    }
                }
                else
                {
                    var entry = serializer.Deserialize<T>(target);
                    yield return entry;
                }
            }
        }

        public static void EnsureObjectStart(this JsonTextReader target)
        {
            if (target.TokenType != JsonToken.StartObject)
            {
                throw new InvalidDataException($"Unexpected JSON Token Type '{GetTokenString(target.TokenType)}'. Expected a JSON Object.");
            }
        }

        public static void EnsureArrayStart(this JsonTextReader target)
        {
            if (target.TokenType != JsonToken.StartArray)
            {
                throw new InvalidDataException($"Unexpected JSON Token Type '{GetTokenString(target.TokenType)}'. Expected a JSON Array.");
            }
        }

        public static int? ReadAsInt32(this JsonTextReader target, string propertyName)
        {
            target.Read();

            if (target.TokenType != JsonToken.Integer)
            {
                throw new InvalidDataException($"Expected '{propertyName}' to be of type {JTokenType.Integer}.");
            }

            if (target.Value == null)
            {
                return null;
            }

            return Convert.ToInt32(target.Value, CultureInfo.InvariantCulture);
        }

        public static string ReadAsString(this JsonTextReader target, string propertyName)
        {
            target.Read();

            if (target.TokenType != JsonToken.String)
            {
                throw new InvalidDataException($"Expected '{propertyName}' to be of type {JTokenType.String}.");
            }

            return target.Value?.ToString();
        }

        public static bool CheckRead(this JsonTextReader target)
        {
            if (!target.Read())
            {
                throw new InvalidDataException("Unexpected end when reading JSON.");
            }

            return true;
        }

        public static bool ReadForType(this JsonTextReader target, Type type)
        {
            // Explicitly read values as dates from JSON with reader.
            // We do this because otherwise dates are read as strings
            // and the JsonSerializer will use a conversion method that won't
            // preserve UTC in DateTime.Kind for UTC ISO8601 dates
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                target.ReadAsDateTime();
            }
            else if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
            {
                target.ReadAsDateTimeOffset();
            }
            else
            {
                target.Read();
            }

            // TokenType will be None if there is no more content
            return target.TokenType != JsonToken.None;
        }
        public static string GetTokenString(this JsonToken target)
        {
            switch (target)
            {
                case JsonToken.None:
                    break;
                case JsonToken.StartObject:
                    return JTokenType.Object.ToString();
                case JsonToken.StartArray:
                    return JTokenType.Array.ToString();
                case JsonToken.PropertyName:
                    return JTokenType.Property.ToString();
                default:
                    break;
            }
            return target.ToString();
        }
    }
}
