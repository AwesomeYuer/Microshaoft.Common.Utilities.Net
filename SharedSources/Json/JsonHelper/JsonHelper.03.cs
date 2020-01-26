// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System.IO;
    public static partial class JsonHelper
    {
      
        public static JObject GetObject(this JToken target)
        {
            if (target == null || target.Type != JTokenType.Object)
            {
                throw new InvalidDataException($"Unexpected JSON Token Type '{target?.Type}'. Expected a JSON Object.");
            }

            return (JObject) target;
        }

        public static T GetOptionalProperty<T>
                    (
                        this JObject target
                        , string propertyName
                        , JTokenType expectedType = JTokenType.None
                        , T defaultValue = default
                    )
        {
            var property = target[propertyName];

            if (property == null)
            {
                return defaultValue;
            }

            return
                property
                    .GetValue<T>
                        (
                            propertyName
                            , expectedType
                        );
        }

        public static T GetRequiredProperty<T>
                        (
                            this JObject target
                            , string propertyName
                            , JTokenType expectedType = JTokenType.None
                        )
        {
            var prop = target[propertyName];

            if (prop == null)
            {
                throw new InvalidDataException($"Missing required property '{propertyName}'.");
            }

            return
                prop
                    .GetValue<T>
                        (
                            propertyName
                            , expectedType
                        );
        }

        public static T GetValue<T>
                                (
                                    this JToken target
                                    , string propertyName
                                    , JTokenType expectedType
                                )
        {
            if 
                (
                    expectedType != JTokenType.None
                    &&
                    target.Type != expectedType
                )
            {
                throw new InvalidDataException($"Expected '{propertyName}' to be of type {expectedType}.");
            }
            return target.Value<T>();
        }
    }
}
