namespace Test
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    class Program12
    {
        static void Main(string[] args)
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
    public static class JsonReaderHelper
    {

        public static void ReadAllMultipleContents
                                         (
                 this JsonReader target)
        {
            if (!target.SupportMultipleContent)
            {
                target.SupportMultipleContent = true;
            }
            var serializer = new JsonSerializer();
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
                            Console.WriteLine();
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
    }
}
