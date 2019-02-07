#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public interface IHttpRequestValidateable<TParameter>
    {
        string Name
        {
            get;
        }
        (
            bool IsValid
            , IActionResult Result
        )
            Validate
                (
                    TParameter parameter
                    , ActionExecutingContext actionExecutingContext
                );
    }
}
#endif