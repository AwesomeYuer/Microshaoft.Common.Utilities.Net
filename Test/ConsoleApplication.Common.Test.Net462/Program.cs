#if !NETSTANDARD1_4
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
            string json = @"
{ a: [{a:'asdasd',b:2222},{a:'@a.[2].a'},{a:'ssss'}]}



";

            string json2 = @"
{a:['asdasd','aaaa',{a:1111}]}



";


            Console.WriteLine
                        (
                            JsonHelper
                                .MergeJsonTemplate
                                    (
                                        json
                                        , json2
                                    )
                        );

            //JsonReader reader = new JsonTextReader(new StringReader(json));
            //reader.ReadAllMultipleContents();
            ////reader.re



            ////foreach (var i in r)
            ////{

            ////    Console.WriteLine(i);
            ////}
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
#endif
