#if NETCOREAPP2_X
namespace Microshaoft
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    public class OptionalProducesAttribute : ProducesAttribute
    {
        public string RequestPathKey
        {
            get;
            set;
        }
        public OptionalProducesAttribute(Type type) : base(type)
        {

        }
        public OptionalProducesAttribute
                    (
                        string contentType
                        , params string[] additionalContentTypes
                    )
            : base(contentType, additionalContentTypes)
        {
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (!RequestPathKey.IsNullOrEmptyOrWhiteSpace())
            {
                var httpContext = context.HttpContext;
                var request = httpContext.Request;
                //var routeName = context.RouteData.Values["routeName"];
                var path = request.Path.ToString();
                var r = path
                            .Contains
                                (
                                    RequestPathKey
                                    , StringComparison
                                            .OrdinalIgnoreCase
                                );
                if (r)
                {
                    base
                        .OnResultExecuting(context);
                }
            }
        }
    }
}
#endif