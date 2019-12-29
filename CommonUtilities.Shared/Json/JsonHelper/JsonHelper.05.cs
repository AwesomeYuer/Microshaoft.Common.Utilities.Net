namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    public static partial class JsonHelper
    {
        public static JToken MergeJsonTemplateToJToken
                 (
                     string jsonTemplate
                     , string jsonData
                     , string jsonTemplatePathPrefix = "@"
                 )
        {
            var jTokenTemplate = JToken.Parse(jsonTemplate);
            var jTokenData = JToken.Parse(jsonData);
            JsonReaderHelper
                    .ReadAllPaths
                        (
                            jsonTemplate
                            ,
                                (
                                    isJArray
                                    , jsonPath
                                    , valueObject
                                    , valueType
                                    , reader
                                ) =>
                                {
                                    if (valueObject is string vs)
                                    {
                                        vs = vs.Trim();
                                        if (vs.StartsWith(jsonTemplatePathPrefix))
                                        {
                                            var replacedSelectToken = jTokenTemplate.SelectToken(jsonPath);
                                            var trimChars = jsonTemplatePathPrefix.ToCharArray();
                                            vs = vs.TrimStart(trimChars);
                                            var replacementSelectToken = jTokenData.SelectToken(vs);
                                            replacedSelectToken.Replace(replacementSelectToken);
                                        }
                                    }
                                    return false;
                                }
                        );
            return jTokenTemplate;
        }
        public static string MergeJsonTemplate
                (
                    string jsonTemplate
                    , string jsonData
                    , string jsonTemplatePathPrefix = "@"
                )
        {

            return
                    MergeJsonTemplateToJToken
                                (
                                    jsonTemplate
                                    , jsonData
                                    , jsonTemplatePathPrefix
                                )
                                .ToString();
        }
    }
}
