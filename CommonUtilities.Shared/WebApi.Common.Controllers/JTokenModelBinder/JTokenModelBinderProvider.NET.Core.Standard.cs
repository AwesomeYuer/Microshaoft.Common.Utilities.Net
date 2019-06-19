﻿#if NETCOREAPP2_X
namespace Microshaoft.WebApi.ModelBinders
{
    using Microshaoft.Web;
    //using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    //using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using Microsoft.Extensions.Configuration;
    //using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;
    //using System;
    using System.Threading.Tasks;
    public class JTokenModelBinder : IModelBinder
    {
        //public JTokenModelBinder()
        //{ }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var httpContext = bindingContext
                                    .HttpContext;
            var request = httpContext
                                    .Request;
            IConfiguration configuration =
                        (IConfiguration) bindingContext
                                            .HttpContext
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
                            //request
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
                                    "requestJTokenParameters"
                                )
                    )
                {
                    httpContext
                            .Items
                            .Add
                                (
                                    "requestJTokenParameters"
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