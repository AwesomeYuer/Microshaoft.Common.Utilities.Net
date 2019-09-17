// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.



namespace Microshaoft
{

    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    public static partial class JsonHelper
    {
      
        public static JObject GetObject(this JToken target)
        {
            if (target == null || target.Type != JTokenType.Object)
            {
                throw new InvalidDataException($"Unexpected JSON Token Type '{target?.Type}'. Expected a JSON Object.");
            }

            return (JObject)target;
        }

        public static T GetOptionalProperty<T>
                    (
                        this JObject target
                        , string property
                        , JTokenType expectedType = JTokenType.None
                        , T defaultValue = default(T)
                    )
        {
            var prop = target[property];

            if (prop == null)
            {
                return defaultValue;
            }

            return prop.GetValue<T>(property, expectedType);
        }

        public static T GetRequiredProperty<T>
                        (
                            this JObject target
                            , string property
                            , JTokenType expectedType = JTokenType.None
                        )
        {
            var prop = target[property];

            if (prop == null)
            {
                throw new InvalidDataException($"Missing required property '{property}'.");
            }

            return prop.GetValue<T>(property, expectedType);
        }

        public static T GetValue<T>(this JToken target, string property, JTokenType expectedType)
        {
            if (expectedType != JTokenType.None && target.Type != expectedType)
            {
                throw new InvalidDataException($"Expected '{property}' to be of type {expectedType}.");
            }
            return target.Value<T>();
        }



        

        
    }
}
