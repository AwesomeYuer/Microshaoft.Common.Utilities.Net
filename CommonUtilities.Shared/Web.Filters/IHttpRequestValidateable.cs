#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public interface IHttpRequestValidateable<TParameters>
    {
        string Name
        {
            get;
        }
        (
            bool IsValid
            ,
            IActionResult Result
        )
            Validate
                (
                    TParameters parameters
                    , ActionExecutingContext actionExecutingContext
                );
    }
}
#endif