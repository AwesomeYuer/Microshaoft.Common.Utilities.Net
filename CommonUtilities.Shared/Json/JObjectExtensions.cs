namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    public static class JObjectExtensions
    {
        public static IEnumerable<JValue> GetAllJValues(this JToken jToken)
        {
            if (jToken is JValue jValue)
            {
                yield return jValue;
            }
            else if (jToken is JArray jArray)
            {
                foreach (var result in GetAllJValuesFromJArray(jArray))
                {
                    yield return result;
                }
            }
            else if (jToken is JProperty jProperty)
            {
                foreach (var result in GetAllJValuesFromJProperty(jProperty))
                {
                    yield return result;
                }
            }
            else if (jToken is JObject jObject)
            {
                foreach (var result in GetAllValuesFromJObject(jObject))
                {
                    yield return result;
                }
            }
        }

        #region Private helpers

        static IEnumerable<JValue> GetAllJValuesFromJArray(JArray jArray)
        {
            for (var i = 0; i < jArray.Count; i++)
            {
                foreach (var result in GetAllJValues(jArray[i]))
                {
                    yield return result;
                }
            }
        }

        static IEnumerable<JValue> GetAllJValuesFromJProperty(JProperty jProperty)
        {
            foreach (var result in GetAllJValues(jProperty.Value))
            {
                yield return result;
            }
        }

        static IEnumerable<JValue> GetAllValuesFromJObject(JObject jObject)
        {
            foreach (var jToken in jObject.Children())
            {
                foreach (var result in GetAllJValues(jToken))
                {
                    yield return result;
                }
            }
        }

        #endregion
    }
}