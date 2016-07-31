namespace Test
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microshaoft;
    class Program11
    {
        static void Main(string[] args)
        {

            string json = @"{ 'name': 
'Admin' }
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
            var r = reader.ReadAllMultipleContent<Role>().ToArray();

            reader = new JsonTextReader(new StringReader(json));
            r = reader
                    .ReadMultipleContent<Role>(3)
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
    using System.Collections.Generic;
    public static class JsonReaderHelper
    {
        public static IEnumerable<IEnumerable<T>> ReadMultipleContent<T>
                        (
                            this JsonReader target
                            , int pageSize = 3
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

        public static IEnumerable<T> ReadAllMultipleContent<T>(this JsonReader target)
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
