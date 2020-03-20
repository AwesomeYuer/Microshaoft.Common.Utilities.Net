namespace Test
{
    using Microshaoft;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    public static class JsonReaderHelper
    {
        public static void ReadAllPaths
                    (
                        string json
                        , Func
                            <
                                bool
                                , string
                                , object
                                , Type
                                , JsonReader
                                , bool
                            >
                                onReadPathOnceProcessFunc
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
                                            this JsonReader @this
                                        )
        {
            if (!@this.SupportMultipleContent)
            {
                @this.SupportMultipleContent = true;
            }
            var serializer = new JsonSerializer();
            //serializer.CheckAdditionalContent
            while (@this.Read())
            {
                Console.WriteLine(@this.TokenType);
                var r = serializer.Deserialize(@this);
                Console.WriteLine(r.GetType());
                Console.WriteLine(r.ToString());
            }
        }
        public static IEnumerable<JToken> ReadMultipleContents
                                                (
                                                    this JsonReader @this
                                                )
        {
            if (!@this.SupportMultipleContent)
            {
                @this.SupportMultipleContent = true;
            }
            var serializer = new JsonSerializer();
            while (@this.Read())
            {
                if (@this.TokenType == JsonToken.StartObject)
                {
                    JToken entry = serializer.Deserialize<JToken>(@this);
                    yield return entry;
                }
                else if (@this.TokenType == JsonToken.StartArray)
                {
                    JArray entries = serializer.Deserialize<JArray>(@this);
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
                            this JsonReader @this
                            , int pageSize = 10
                        )
        {
            if (!@this.SupportMultipleContent)
            {
                @this.SupportMultipleContent = true;
            }
            var serializer = new JsonSerializer();
            var list = new List<T>();
            var i = 0;
            while (@this.Read())
            {
                if (@this.TokenType == JsonToken.StartArray)
                {
                    var entries = serializer.Deserialize<T[]>(@this);
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
                    var entry = serializer.Deserialize<T>(@this);
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
                yield
                    return
                        list;
                list.Clear();
            }
        }

        public static IEnumerable<T> ReadAllMultipleContentsAsEnumerable<T>(this JsonReader @this)
        {
            if (!@this.SupportMultipleContent)
            {
                @this.SupportMultipleContent = true;
            }
            var serializer = new JsonSerializer();
            while (@this.Read())
            {
                if (@this.TokenType == JsonToken.StartArray)
                {
                    var entries = serializer.Deserialize<T[]>(@this);
                    foreach (var entry in entries)
                    {
                        yield return entry;
                    }
                }
                else
                {
                    var entry = serializer.Deserialize<T>(@this);
                    yield return entry;
                }
            }
        }

        public static void EnsureObjectStart(this JsonTextReader @this)
        {
            if (@this.TokenType != JsonToken.StartObject)
            {
                throw new InvalidDataException($"Unexpected JSON Token Type '{GetTokenString(@this.TokenType)}'. Expected a JSON Object.");
            }
        }

        public static void EnsureArrayStart(this JsonTextReader @this)
        {
            if (@this.TokenType != JsonToken.StartArray)
            {
                throw new InvalidDataException($"Unexpected JSON Token Type '{GetTokenString(@this.TokenType)}'. Expected a JSON Array.");
            }
        }

        public static int? ReadAsInt32(this JsonTextReader @this, string propertyName)
        {
            @this.Read();

            if (@this.TokenType != JsonToken.Integer)
            {
                throw new InvalidDataException($"Expected '{propertyName}' to be of type {JTokenType.Integer}.");
            }

            if (@this.Value == null)
            {
                return null;
            }

            return Convert.ToInt32(@this.Value, CultureInfo.InvariantCulture);
        }

        public static string ReadAsString(this JsonTextReader @this, string propertyName)
        {
            @this.Read();

            if (@this.TokenType != JsonToken.String)
            {
                throw new InvalidDataException($"Expected '{propertyName}' to be of type {JTokenType.String}.");
            }

            return @this.Value?.ToString();
        }

        public static bool CheckRead(this JsonTextReader @this)
        {
            if (!@this.Read())
            {
                throw new InvalidDataException("Unexpected end when reading JSON.");
            }

            return true;
        }

        public static bool ReadForType(this JsonTextReader @this, Type type)
        {
            // Explicitly read values as dates from JSON with reader.
            // We do this because otherwise dates are read as strings
            // and the JsonSerializer will use a conversion method that won't
            // preserve UTC in DateTime.Kind for UTC ISO8601 dates
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                @this.ReadAsDateTime();
            }
            else if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
            {
                @this.ReadAsDateTimeOffset();
            }
            else
            {
                @this.Read();
            }

            // TokenType will be None if there is no more content
            return @this.TokenType != JsonToken.None;
        }
        public static string GetTokenString(this JsonToken @this)
        {
            switch (@this)
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
            return @this.ToString();
        }
    }
}
