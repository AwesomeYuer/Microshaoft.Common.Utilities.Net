//#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;

    public static partial class JsonHelper
    {
        public static bool FreezeValueProperty
                                    (
                                        this JObject target
                                        , string propertyName
                                        , int afterChangedTimes = 0
                                    )
        {
            var r = false;
            //var property = target[propertyName];
            //if (property != null)
            //{
            //if (property is JValue)
            //{
            int changed = 0;
            target
                .PropertyChanged +=
                            (
                                (x, y) =>
                                {
                                    if (y.PropertyName == propertyName)
                                    {
                                        Interlocked.Increment(ref changed);
                                    }
                                }
                            );
            target
                .PropertyChanging +=
                        (
                            (x, y) =>
                            {
                                if (y.PropertyName == propertyName)
                                {
                                    if (changed >= afterChangedTimes)
                                    {
                                        throw new Exception
                                                    (
                                                        $"Property {propertyName}'s value is freezed"
                                                    );
                                    }
                                }
                            }
                        );
            r = true;
            //}

            //}


            return r;
        }
        private static void Travel
                                (
                                    JObject rootJObject
                                    , string ancestorPath
                                    , string propertyName
                                    , JObject currentJObject
                                    , Action<JObject, string, string, JObject, JProperty>
                                            onTraveledJValuePropertyProcessAction = null
                                    , Action<JObject, string, string, JObject>
                                            onTraveledJObjectPropertyProcessAction = null
                                )
        {
            onTraveledJObjectPropertyProcessAction?
                    .Invoke
                        (
                            rootJObject
                            , ancestorPath
                            , propertyName
                            , currentJObject
                        );
            var jProperties = currentJObject.Properties();
            foreach (var jProperty in jProperties)
            {
                var path = ancestorPath;
                if
                    (
                        !string.IsNullOrEmpty(path)
                        &&
                        !string.IsNullOrWhiteSpace(path)
                    )
                {
                    path += ".";
                }
                path += jProperty.Name;
                if (jProperty.HasValues)
                {
                    if
                        (
                            jProperty.Value is JValue
                        )
                    {
                        onTraveledJValuePropertyProcessAction?
                                .Invoke
                                    (
                                        rootJObject
                                        , ancestorPath
                                        , jProperty.Name
                                        , currentJObject
                                        , jProperty
                                    );
                    }
                    else if
                        (
                            jProperty.Value is JArray
                        )
                    {
                        var jArray = jProperty.Value as JArray;
                        var i = 0;
                        foreach (var jToken in jArray)
                        {
                            if (jToken is JObject jObject)
                            {
                                Travel
                                    (
                                        rootJObject
                                        , string.Format("{0}[{1}]", path, i)
                                        , jProperty.Name
                                        , jObject
                                        , onTraveledJValuePropertyProcessAction
                                        , onTraveledJObjectPropertyProcessAction
                                    );
                            }
                            i++;
                        }
                    }
                    else
                    {
                        if (jProperty.Value is JObject jObject)
                        {
                            Travel
                                (
                                    rootJObject
                                    , path
                                    , jProperty.Name
                                    , jObject
                                    , onTraveledJValuePropertyProcessAction
                                    , onTraveledJObjectPropertyProcessAction
                                );
                        }
                    }
                }
            }
        }
        public static void Travel
                                (
                                    this JObject target
                                    , Action<JObject, string, string, JObject, JProperty>
                                            onTraveledJValuePropertyProcessAction = null
                                    , Action<JObject, string, string, JObject>
                                            onTraveledJObjectPropertyProcessAction = null
                                )
        {
            Travel
                (
                    target
                    , string.Empty
                    , string.Empty
                    , target
                    , onTraveledJValuePropertyProcessAction
                    , onTraveledJObjectPropertyProcessAction
                );
        }
        public static void SetPropertyValue
                            (
                                this JObject target
                                , string path
                                , JValue value
                                , Action
                                    <
                                        JObject
                                    >
                                        onPropertyChangedProcessAction
                            )
        {
            var a = path.Split('.');
            JToken jToken = target[a[0]];
            for (var i = 1; i < a.Length - 1; i++)
            {
                jToken = jToken[a[i]];
            }

            JObject j = jToken as JObject;
            j[a[a.Length - 1]] = value;

            onPropertyChangedProcessAction?.Invoke(target);
        }
        public static void Observe
                                (
                                    this JObject target
                                    , Action
                                        <
                                            JObject
                                            , string
                                            , JObject
                                            , PropertyChangedEventArgs
                                            , JValue
                                            , JValue
                                        >
                                            onPropertyChangedProcessAction
                                )
        {

            //target.Annotation<Action<string>>().
            // HashSet --> Dictionary
            target.AddAnnotation(new Dictionary<string, JObject>());
            target.PropertyChanged += (
                (sender, args) =>
                {
                    if (!target.Annotation<Dictionary<string, JObject>>().ContainsKey(args.PropertyName))
                    {
                        JObject jDelegate = new JObject
                        (
                            new JProperty("methodName", onPropertyChangedProcessAction.Method.Name)
                            , new JProperty("isFlag", onPropertyChangedProcessAction == null ? true : false)
                        );
                        target.Annotation<Dictionary<string, JObject>>().Add(args.PropertyName, jDelegate);
                    }
                }
            );

            Travel
                (
                    target
                    , null
                    , (root, path, paropertyName, current) =>
                    {
                        JValue lastValue = null;
                        current
                            .PropertyChanging +=
                                            (
                                                (sender, args) =>
                                                {
                                                    if (sender is JObject jo)
                                                    {
                                                        lastValue = jo[args.PropertyName] as JValue;
                                                    }
                                                }
                                            );
                        JValue newValue = null;
                        current
                            .PropertyChanged +=
                                            (
                                                (sender, args) =>
                                                {
                                                    if (sender is JObject jo)
                                                    {
                                                        newValue = jo[args.PropertyName] as JValue;
                                                    }
                                                    onPropertyChangedProcessAction
                                                            (
                                                                root
                                                                , path
                                                                , current
                                                                , args
                                                                , lastValue
                                                                , newValue
                                                            );
                                                }
                                            );
                    }
                );
        }
    }
}
namespace Test
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using Microshaoft;
    class Program12
    {
        static void Main11111(string[] args)
        {
            var root = new JTree
            {
                Teller = new Teller()
            };
            root.Teller.Name = "2222";


            root.Teller.TellerNo = "11111";
            root.Teller.TellerNo = "222";

            root.Teller.Other = "asdasdsa";




            try
            {
                root.Teller.TellerNo = "333";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            root.Teller = new Teller();
            root.Teller.TellerNo = "zzzz";
            Console.WriteLine(root.Teller.TellerNo);
            Console.WriteLine(root.ToString());
            Console.Read();
        }

    }

    public class JTree : JObject
    {
        public dynamic Teller
        {
            get
            {
                return this["Teller"];
            }
            set
            {
                this["Teller"] = value;
            }

        }


    }

    public class Teller : JObject
    {

        public string TellerNo
        {
            get
            {
                return this["TellerNo"].Value<string>();
            }
            set
            {
                this["Teller"] = value;
            }
        }

        public Teller()
        {
            this.FreezeValueProperty("TellerNo", 2);
        }
    }
}
namespace TestConsoleApp6
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System;
    class Program
    {
        static void Main1(string[] args1)
        {
            string json = @"{
  'Name': 'Bad Boys',
  'ReleaseDate': '1995-4-7T00:00:00',
  'Genres': [
    {F1:'Action'},
    'Comedy'
  ]
,F4:{F1:'aa',F2:'ddd'}
}";

            json = "{Name:'asdsadas',F2:[1,{F1:'00000'}],F3: null, F4:{F5:{F6:{F7:'000',F8:'1'},F8:null}}}";
            var jObject = JObject.Parse(json);

            //jObject.Observe
            //            (
            //                (root, ancestorPath, current, args, v1, v2) =>
            //                {
            //                    Console.WriteLine("===================");
            //                    Console.WriteLine
            //                                (
            //                                    "{0}.{1}: ({2}) => ({3})"
            //                                    , ancestorPath
            //                                    , args.PropertyName
            //                                    , v1
            //                                    , v2
            //                                );
            //                    Console.WriteLine("===================");
            //                }
            //            );



            //jObject["name"] = "zzzz";
            //jObject["name"] = "1111";

            //Console.WriteLine(jObject.ToString());

            //jObject["F4"]["F5"]["F6"] = "zzzzA";

            //jObject[@"F9"] = "asdsa";

            //jObject["F2"][1]["F1"] = "zzzzA0000";


            //jObject["F2"][1]["F90"] = JObject.Parse("{'a':1,'b':2}");
            //jObject["F3"] = "zzzz";

            jObject.SetPropertyValue("F4.F5.F6.F7", new JValue("666"), null);

            Console.WriteLine(
                    jObject.ToString()
            );


            Console.ReadLine();
        }
    }
}
//#endif