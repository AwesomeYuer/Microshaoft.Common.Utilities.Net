namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.ComponentModel;

    public static class JObjectExtensionMethodsManager
    {
        private static void TravelValueProperties
                                (
                                    JObject rootJObject
                                    , string rootPath
                                    , JObject currentJObject
                                    , Action<JObject, string, JObject, JProperty> onTraveledProcessAction = null
                                )
        {
            var jProperties = currentJObject.Properties();

            foreach (var jProperty in jProperties)
            {
                var path = rootPath + "." + jProperty.Name;
                if (jProperty.HasValues)
                {
                    if
                        (
                            jProperty.Value is JValue
                        )
                    {
                        onTraveledProcessAction?
                                .Invoke
                                    (
                                        rootJObject
                                        , path
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
                            var jObject = jToken as JObject;
                            if (jObject != null)
                            {
                                TravelValueProperties
                                    (
                                        rootJObject
                                        , string.Format("{0}[{1}]", path, i)
                                        , jObject
                                        , onTraveledProcessAction
                                    );
                            }
                            i++;
                        }
                    }
                    else
                    {
                        var jObject = jProperty.Value as JObject;
                        if (jObject != null)
                        {
                            TravelValueProperties
                                (
                                    rootJObject
                                    , path
                                    , jObject
                                    , onTraveledProcessAction
                                );
                        }
                    }
                }
            }
        }


        public static void TravelValueProperties
                                (
                                    this JObject target
                                    , Action<JObject, string, JObject, JProperty> onTraveledProcessAction = null
                                )
        {
            TravelValueProperties(target, "", target, onTraveledProcessAction);
        }




        public static void AddEventsListener
                                (
                                    this JObject target
                                    , Action<JObject, string, JObject, PropertyChangedEventArgs, JValue, JValue>
                                        onPropertyChangedProcessAction
                                )
        {
            TravelValueProperties
                (
                    target
                    , (root, path, current, property) =>
                    {
                        JValue lastValue = null;
                        current.PropertyChanging +=
                                            (
                                                (sender, args) =>
                                                {
                                                    var jo = sender as JObject;
                                                    if (jo != null)
                                                    {
                                                        lastValue = jo[args.PropertyName] as JValue;
                                                    }
                                                }
                                            );
                        JValue newValue = null;
                        current.PropertyChanged +=
                                (
                                    (sender, args) =>
                                    {
                                        var jo = sender as JObject;
                                        if (jo != null)
                                        {
                                            newValue = jo[args.PropertyName] as JValue;
                                        }
                                        onPropertyChangedProcessAction(root, path, current, args, lastValue, newValue);
                                    }
                                );
                    }
                );

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
        static void Main(string[] args1)
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

            json = "{Name:'asdsadas',F2:'00000',F3: null, F4:{F5:{F6:null}}}";
            var jObject = JObject.Parse(json);

            jObject.AddEventsListener
                        (
                            (x, y, z, zz, v1, v2) =>
                            {
                                Console.WriteLine(zz.PropertyName);
                                Console.WriteLine(y);
                                Console.WriteLine(v1);
                                Console.WriteLine(v2);
                                Console.WriteLine("===================");
                            }
                        );

            jObject.TravelValueProperties
                        (
                            (root, path, current, property) =>
                            {
                                Console.WriteLine(path);
                            }
                        );

            //jObject.AddEventsListener();

            //jObject["Name"] = "zzzz";
            // jObject["F1"][0]["F1"] = "zzzz";
            //jObject["F3"] = "zzzz";

            //Console.WriteLine(jObject["F1"][0]["F1"]);


            Console.ReadLine();
        }




    }
}
