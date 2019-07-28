namespace Microshaoft.Web
{
    using System.Net.Http;
    public static class HttpRequestMessageExtensions
    {
        public static bool TryGetPropertyValue<T>
                                (
                                    this HttpRequestMessage target
                                    , string propertyKey
                                    , out T propertyValue
                                )
        {
            propertyValue = default;
            bool r = target
                        .Properties
                        .TryGetValue
                            (
                                propertyKey
                                , out object propertyValueObject
                            );
            if (r)
            { 
                propertyValue = (T) propertyValueObject;
            }
            return r;
        }
        public static T GetPropertyValue<T>
                        (
                            this HttpRequestMessage target
                            , string propertyKey
                        )
        {
            var propertyValue = default(T);
            bool r = target
                        .Properties
                        .TryGetValue
                            (
                                propertyKey
                                , out object propertyValueObject
                            );
            if (r)
            {
                propertyValue = (T) propertyValueObject;
            }
            return propertyValue;
        }
    }
}