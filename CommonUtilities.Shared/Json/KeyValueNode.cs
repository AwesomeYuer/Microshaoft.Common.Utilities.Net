
namespace Test
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Diagnostics;
    class Program1111
    {
        static void Main115(string[] args)
        {
            var json = @"
[
	{""B"":{""I"":1111,""M"":""你好"",""O"":""Operation"",""S"":1000},""H"":{""T"":null,""RT"":null,""SEQ"":null,""RSEQ"":null,""I"":null,""L"":null,""RR"":null,""S"":null,""R"":[{""A"":""app1"",""G"":""group1"",""U"":""user1""}],""RO"":null,""ST"":null,""ET"":""2015-01-26 17:19:30.3560275+08:00"",""SC"":null,""RV"":null}}
	,
	{""B1"":{""I"":1111,""M"":""你好"",""O"":""Operation"",""S"":1000},""H"":{""T"":null,""RT"":null,""SEQ"":null,""RSEQ"":null,""I"":null,""L"":null,""RR"":null,""S"":null,""R"":[{""A"":""app1"",""G"":""group1"",""U"":""user1""},{""A"":""app1"",""G"":""group1"",""U"":""user1""}],""RO"":null,""ST"":null,""ET"":""2015-01-26 17:19:30.3560275+08:00"",""SC"":null,""RV"":null}}
]";
            json = @"{F2: [{a:""asdsad""}],F1: ""F1f1f1f1f1"",""B"":{""I"":999,""M"":""你好"",""O"":""Operation"",""S"":1000},""H"":{""T"":null,""RT"":1111111,""SEQ"":null,""RSEQ"":null,""RR"":null,""S"":null,""R"":[{""A"":""app1"",""G"":""group1"",""U"":""user1""}],""RO"":null,""ST"":null,""ET"":""2015-01-26 17:19:30.3560275+08:00"",""SC"":null,""RV"":null}}";
            json = @"
{
	F1 : ""F1.Value"" ,
	F2 :
            {
                F2 : ""F2.F2.Value""
            } ,
	F3 :
            [
                {
                    F1 : ""F3[0].F1.Value"" ,
                    F2 : ""F3[0].F2.Value""
                } ,
                {
                    F4 : ""F3[0].F4.Value"" ,
                    F5 : ""F3[1].F5.Value""
                }
            ] ,
    F4 :
            [
                ""1"" ,
                ""2""
            ]
}
";

            var jToken = JToken.Parse(json);
            KeyValueNode node = KeyValueNode.CreateFromJToken(jToken, null);

            Console.WriteLine(node["F1"]);
            Console.WriteLine(node["F2"]["F2"].Value);
            Console.WriteLine(node["F3"][0]["F1"].Value);
            Console.WriteLine(node["F3"][1]["F5"]);
            Console.WriteLine(node["F4"][1].Value);

            node["F1"].Value += " new";
            node["F2"]["F2"].Value += " new";
            node["F3"][0]["F1"].Value += " new";
            node["F3"][1]["F5"].Value += " new";
            node["F4"][1].Value += " new";

            Console.WriteLine(node["F1"]);
            Console.WriteLine(node["F2"]["F2"].Value);
            Console.WriteLine(node["F3"][0]["F1"].Value);
            Console.WriteLine(node["F3"][1]["F5"]);
            Console.WriteLine(node["F4"][1].Value);

            Console.WriteLine("==========================");
            json = node.ToJson(false);
            Console.WriteLine(json);
            json = node.ToJson();
            Console.WriteLine(json);
            //json = JsonConvert.SerializeObject(node);

            var jObject = new JObject();
            jObject.Add("Entered", DateTime.Now);
            dynamic album = jObject;
            album.AlbumName = "Dirty Deeds Done Dirt Cheap";
            album.Artist = "AC/DC";
            album.YearReleased = 1976;
            album.Songs = new JArray() as dynamic;
            dynamic song = new JObject();
            song.SongName = "Dirty Deeds Done Dirt Cheap";
            song.SongLength = "4:11";
            album.Songs.Add(song);
            song = new JObject();
            song.SongName = "Love at First Feel";
            song.SongLength = "3:10";
            album.Songs.Add(song);
            node = KeyValueNode
                        .CreateFromJToken
                            (
                                (JToken)album
                                , null
                            );
            Console.WriteLine
                (
                    node["Songs"][0]["SongName"].Value
                );

            Console.WriteLine("==========================");

            node["Songs"][0]["SongName"].Value += " new";
            json = node.ToJson(false);
            Console.WriteLine(json);
            json = node.ToJson();
            Console.WriteLine(json);


            Console.ReadLine();
        }
        private static void Print(KeyValueNode jsonNode)
        {
            Debug.WriteLine(string.Format("{0}:{1}", jsonNode.GetType(), jsonNode));

            if (jsonNode.Children != null)
            {

                Debug.Indent();
                foreach (KeyValueNode childJsonNode in jsonNode.Children)
                {
                    Print(childJsonNode);
                }
                Debug.Unindent();
            }
        }
    }
}
namespace Microshaoft
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public abstract class KeyValueNode
    {


        public KeyArrayValueNode AsKeyArrayValueNode
        {
            protected set;
            get;
        }

        public KeyObjectValueNode AsKeyObjectValueNode
        {
            protected set;
            get;
        }
        public KeyValuePairNode AsKeyValuePairNode
        {
            protected set;
            get;
        }

        public ObjectValueNode AsObjectValueNode
        {
            protected set;
            get;
        }

        public KeyValueNode Parent
        {
            set;
            get;
        }
        public KeyValueNode Instance
        {
            set;
            get;
        }
        public IEnumerable<KeyValueNode> Children
        {
            get;
            internal set;
        }

        public JToken NodeJToken
        {
            get;
            set;
        }

        protected bool TryGetJValue(JToken jToken, out JValue jValue)
        {
            var r = false;
            jValue = null;
            if
                (
                    NodeJToken is JProperty
                )
            {
                var jProperty = ((JProperty)NodeJToken);
                if
                    (
                        jProperty.Value is JValue
                    )
                {
                    jValue = (JValue)jProperty.Value;
                    r = true;
                }
            }
            else if
                (
                    NodeJToken is JValue
                )
            {
                jValue = (JValue)NodeJToken;
                r = true;
            }
            return r;
        }
        public virtual object Value
        {
            get
            {
                JValue r = null;
                if (TryGetJValue(NodeJToken, out r))
                {
                    return r;
                }
                throw new NotImplementedException();
            }
            set
            {
                if (!TrySetValue(value))
                {
                    throw new NotImplementedException();
                }
            }
        }
        protected bool TrySetValue
                        (
                            object value
                        )
        {
            var r = false;
            JToken jToken = NodeJToken;
            if (jToken is JProperty)
            {
                JProperty jProperty = ((JProperty)jToken);
                if
                    (
                        jProperty.Value is JValue
                    )
                {
                    jProperty.Value = new JValue(value);
                    r = true;
                }
            }
            else
            {
                //数组
                jToken = Parent.NodeJToken;
                if (jToken is JProperty)
                {
                    var jProperty = ((JProperty)jToken);
                    if (jProperty.Value is JArray)
                    {
                        if (NodeJToken is JValue)
                        {
                            var jArray = (JArray)jProperty.Value;
                            ObjectValueNode jsonValueNode = (ObjectValueNode)this;
                            jArray[jsonValueNode.ArrayElementIndex] = new JValue(value);
                        }
                        else
                        {
                            NodeJToken = new JValue(value);
                        }
                        r = true;
                    }
                }
            }
            return r;
        }
        public string ToJson(bool needKeyQuote = true)
        {
            if (needKeyQuote)
            {
                return
                    NodeJToken.ToString();
            }
            else
            {
                string json = string.Empty;
                using (StringWriter stringWriter = new StringWriter())
                {
                    using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
                    {
                        jsonTextWriter.QuoteName = needKeyQuote;
                        JsonSerializer jsonSerializer = new JsonSerializer();
                        jsonSerializer.Serialize(jsonTextWriter, NodeJToken);
                        json = stringWriter.ToString();
                    }
                }
                return json;
            }
        }
        private Dictionary<string, KeyValueNode>
                        _stringsKeysValuesDictionary
                                = new Dictionary<string, KeyValueNode>();
        //private Dictionary<int, KeyValueNode>
        //        _intsKeysValuesDictionary
        //                = new Dictionary<int, KeyValueNode>();
        public void AddChild
                        (
                            string key
                            , KeyValueNode jsonNode
                        )
        {
            _stringsKeysValuesDictionary
                .Add
                    (
                        key
                        , jsonNode
                    );
        }


        public KeyValueNode this[string key]
        {
            get
            {
                return
                    _stringsKeysValuesDictionary[key];
            }
            set
            {
                //_dictionary = value;
            }
        }
        public KeyValueNode[] NodesArray
        {
            set;
            get;

        }
        public KeyValueNode this[int index]
        {
            get
            {
                return
                    NodesArray[index];
            }
            set
            {

            }
        }

        public override string ToString()
        {
            string r = string.Empty;
            JValue jValue = null;
            if
                (
                    TryGetJValue(NodeJToken, out jValue)
                )
            {
                r = jValue.Value.ToString();
            }
            else
            {
                r = base.ToString();
            }
            return r;
        }

        public static KeyValueNode CreateFromJToken
                                (
                                    JToken jToken
                                    , KeyValueNode parentJsonNode
                                    , int arrayElementIndex = -1
                                )
        {
            if (jToken is JValue)
            {
                var jsonValueNode
                            = new ObjectValueNode
                                        (
                                            jToken
                                            , ((JValue)jToken).Value
                                            , parentJsonNode
                                        );
                if (parentJsonNode is KeyArrayValueNode)
                {
                    jsonValueNode.ArrayElementIndex = arrayElementIndex;
                }
                return jsonValueNode;
            }
            else if (jToken is JProperty)
            {
                JProperty jProperty = (JProperty)jToken;
                if (jProperty.Value is JValue)
                {
                    var jsonPropertyNode
                            = new KeyValuePairNode
                                    (
                                        jToken
                                        , jProperty.Name
                                        ,
                                            (
                                                (JValue)jProperty.Value
                                            ).Value
                                        , parentJsonNode
                                    );
                    if (parentJsonNode != null)
                    {
                        parentJsonNode
                            .AddChild
                                (
                                    jProperty.Name
                                    , jsonPropertyNode
                                );
                    }
                    return jsonPropertyNode;
                }
                else if (jProperty.Value is JArray)
                {
                    var jsonArrayNode
                            = new KeyArrayValueNode
                                    (
                                        jToken
                                        , jProperty.Name
                                        , parentJsonNode
                                    );
                    if (parentJsonNode != null)
                    {
                        parentJsonNode
                            .AddChild
                                (
                                    jProperty.Name
                                    , jsonArrayNode
                                );
                    }
                    int i = 0;
                    jsonArrayNode
                        .Children
                                =
                                    (
                                        (JArray)jProperty.Value
                                    )
                                    .Children()
                                    .Select
                                        (
                                            (x) =>
                                            {
                                                var r = CreateFromJToken
                                                            (
                                                                x
                                                                , jsonArrayNode
                                                                , i
                                                            );
                                                i++;
                                                return r;
                                            }
                                        );
                    jsonArrayNode
                        .NodesArray
                            = jsonArrayNode
                                    .Children
                                    .ToArray();
                    return jsonArrayNode;
                }
                else if (jProperty.Value is JObject)
                {
                    var jsonObjectNode
                            = new KeyObjectValueNode
                                        (
                                            jToken
                                            , jProperty.Name
                                            , parentJsonNode
                                        );
                    if (parentJsonNode != null)
                    {
                        parentJsonNode
                            .AddChild
                                (
                                    jProperty.Name
                                    , jsonObjectNode
                                );
                    }
                    jsonObjectNode
                        .Children =
                                    (
                                        (JObject)jProperty.Value
                                    )
                                    .Children()
                                    .Select
                                        (
                                            (x) =>
                                            {
                                                var r = CreateFromJToken
                                                            (
                                                                x
                                                                , jsonObjectNode
                                                            );
                                                return r;
                                            }
                                        );
                    jsonObjectNode
                            .Children
                            .ToList();
                    return jsonObjectNode;
                }
                else
                {
                    throw new Exception("Unknown JProperty");
                }
            }
            else if (jToken is JArray)
            {
                var jsonArrayNode = new KeyArrayValueNode
                                                        (
                                                            jToken
                                                            , null
                                                            , parentJsonNode
                                                        );
                jsonArrayNode
                    .Children =
                                (
                                    (JArray)jToken
                                )
                                .Children()
                                .Select
                                    (
                                        (x) =>
                                        {
                                            var r = CreateFromJToken
                                                        (
                                                             x
                                                             , jsonArrayNode
                                                        );
                                            return r;
                                        }
                                    );
                jsonArrayNode
                        .Children
                        .ToList();
                return jsonArrayNode;
            }
            else if (jToken is JObject)
            {
                KeyObjectValueNode jsonObjectNode = new KeyObjectValueNode
                                                                (
                                                                    jToken
                                                                    , null
                                                                    , parentJsonNode
                                                                );
                jsonObjectNode
                        .Children =
                                    (
                                        (JObject)jToken
                                    )
                                    .Children()
                                    .Select
                                        (
                                            (x) =>
                                            {
                                                var r = CreateFromJToken
                                                            (
                                                                x
                                                                , jsonObjectNode
                                                            );
                                                return r;
                                            }
                                        );
                jsonObjectNode
                        .Children
                        .ToList();
                return jsonObjectNode;
            }
            else
            {
                throw new Exception("Unknown JToken");
            }
        }

    }
    public class ObjectValueNode : KeyValueNode
    {
        public int ArrayElementIndex
        {
            set;
            get;
        }
        public ObjectValueNode
                    (
                        JToken jToken
                        , object value
                        , KeyValueNode parentJsonNode
                    )
        {
            Instance = this;
            AsObjectValueNode = this;
            Parent = parentJsonNode;
            NodeJToken = jToken;
            //Value = value;
        }
        public override string ToString()
        {
            return Value != null ? Value.ToString() : "<null>";
        }

    }
    public class KeyValuePairNode : KeyValueNode
    {
        public string Key
        {
            get;
            private set;
        }

        public KeyValuePairNode
                    (
                        JToken jToken
                        , string name
                        , object value
                        , KeyValueNode parentJsonNode
                    )
        {
            Instance = this;
            AsKeyValuePairNode = this;
            Parent = parentJsonNode;
            NodeJToken = jToken;
            Key = name;
            //_value = value;
        }

    }
    public class KeyArrayValueNode : KeyValueNode
    {
        public string Key
        {
            get;
            private set;
        }
        public KeyArrayValueNode
                    (
                        JToken jToken
                        , string name
                        , KeyValueNode parentJsonNode
                    )
        {
            Instance = this;
            AsKeyArrayValueNode = this;
            Parent = parentJsonNode;
            NodeJToken = jToken;
            Key = name;
        }
    }
    public class KeyObjectValueNode : KeyValueNode
    {
        public string Key
        {
            get;
            private set;
        }
        public KeyObjectValueNode
                        (
                            JToken jToken
                            , string name
                            , KeyValueNode parentJsonNode
                        )
        {
            Instance = this;
            AsKeyObjectValueNode = this;
            Parent = parentJsonNode;
            NodeJToken = jToken;
            Key = name;
        }
    }
}
