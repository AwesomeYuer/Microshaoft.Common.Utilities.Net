#if NETCOREAPP2_X || NETCOREAPP3_X
namespace Microshaoft.WebApi.ModelBinders
{
    using Microshaoft.Web;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;
    public class JTokenModelBinder : IModelBinder
    {
        //public JTokenModelBinder()
        //{ }

        private const string _itemKeyOfRequestJTokenParameters = "requestJTokenParameters";

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var httpContext = bindingContext
                                    .HttpContext;
            var request = httpContext
                                    .Request;
            IConfiguration configuration =
                        (IConfiguration) httpContext
                                            .RequestServices
                                            .GetService
                                                (
                                                    typeof(IConfiguration)
                                                );
            var jwtTokenName = configuration
                                    .GetSection("TokenName")
                                    .Value;
            var ok = false;
            ok = request
                    .TryParseJTokenParameters
                        (
                            out JToken parameters
                            , out var secretJwtToken
                            , () =>
                            {
                                return
                                    bindingContext
                                        .GetFormJTokenAsync();
                            }
                            , jwtTokenName
                        );
            if (ok)
            {
                if
                    (
                        !httpContext
                            .Items
                            .ContainsKey
                                (
                                    _itemKeyOfRequestJTokenParameters
                                )
                    )
                {
                    httpContext
                            .Items
                            .Add
                                (
                                    _itemKeyOfRequestJTokenParameters
                                    , parameters
                                );
                }
            }
            if (!StringValues.IsNullOrEmpty(secretJwtToken))
            {
                httpContext
                        .Items
                        .Add
                            (
                                jwtTokenName
                                , secretJwtToken
                            );
            }
            bindingContext
                    .Result =
                        ModelBindingResult
                                        .Success
                                            (
                                                parameters
                                            );
        }
    }
}
#endif
