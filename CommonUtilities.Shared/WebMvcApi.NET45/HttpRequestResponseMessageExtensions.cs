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
            var r = false;
            object propertyValueObject = null;
            propertyValue = default(T);
            r =
                (
                    target
                    .Properties
                    .TryGetValue
                        (
                            propertyKey
                            , out propertyValueObject
                        )
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
            var r = false;
            object propertyValueObject = null;
            var propertyValue = default(T);
            r =
                (
                    target
                        .Properties
                        .TryGetValue
                            (
                                propertyKey
                                , out propertyValueObject
                            )
                );
            if (r)
            {
                propertyValue = (T)propertyValueObject;
            }
            return propertyValue;
        }


       
    }
}