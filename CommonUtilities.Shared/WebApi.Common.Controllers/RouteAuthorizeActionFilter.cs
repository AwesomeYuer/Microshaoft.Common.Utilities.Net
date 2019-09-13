#if NETCOREAPP2_X
namespace Microshaoft.Web
{

    using Microsoft.AspNetCore.Mvc.Filters;
    using System.Diagnostics;
    using System.Linq;

    public class RouteAuthorizeActionFilter : IActionFilter
    //, IAuthorizationFilter

    {
        private string[] _actionParametersNames;
        public RouteAuthorizeActionFilter
                        (
                                string[] actionParametersNames
                        )
        {
            Debugger.Break();
            _actionParametersNames = actionParametersNames;

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            Debugger.Break();
            //throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {

            


            //throw new NotImplementedException();
            Debugger.Break();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var parameters = context
                        .ActionDescriptor
                        .Parameters
                        .Where
                            (
                                (x) =>
                                {
                                    return
                                        _actionParametersNames
                                                .Any
                                                    (
                                                        (xx) =>
                                                        {
                                                            return
                                                                xx == x.Name;
                                                        }
                                                    );
                                }
                            );

            foreach (var parameter in parameters)
            {
                Debugger.Break();
            }
        }

       
    }
}
#endif