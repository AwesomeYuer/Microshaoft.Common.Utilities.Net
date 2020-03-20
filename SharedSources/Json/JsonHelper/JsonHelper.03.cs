// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System.IO;
    public static partial class JsonHelper
    {
      
        public static JObject GetObject(this JToken @this)
        {
            if 
                (
                    @this == null
                    ||
                    @this.Type != JTokenType.Object
                )
            {
                throw new InvalidDataException($"Unexpected JSON Token Type '{@this?.Type}'. Expected a JSON Object.");
            }

            return (JObject) @this;
        }

        public static T GetOptionalProperty<T>
                    (
                        this JObject @this
                        , string propertyName
                        , JTokenType expectedType = JTokenType.None
                        , T defaultValue = default
                    )
        {
            var property = @this[propertyName];

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
                            this JObject @this
                            , string propertyName
                            , JTokenType expectedType = JTokenType.None
                        )
        {
            var prop = @this[propertyName];

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
                                    this JToken @this
                                    , string propertyName
                                    , JTokenType expectedType
                                )
        {
            if 
                (
                    expectedType != JTokenType.None
                    &&
                    @this.Type != expectedType
                )
            {
                throw new InvalidDataException($"Expected '{propertyName}' to be of type {expectedType}.");
            }
            return @this.Value<T>();
        }
    }
}
