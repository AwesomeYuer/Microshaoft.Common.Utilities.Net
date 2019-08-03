#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;


    public class JTokenParametersValidateFilterAttribute
                                :
                                    //AuthorizeAttribute
                                    Attribute
                                    , IActionFilter
    {
        private class FullTypeNameEqualityComparer<T> : IEqualityComparer<T>
        {
            public bool Equals(T x, T y)
            {
                return
                    (
                        x
                            .GetType()
                            .FullName
                        ==
                        y
                            .GetType()
                            .FullName
                    );
            }

            public int GetHashCode(T x)
            {
                return
                    x
                        .GetType()
                        .FullName
                        .GetHashCode();
            }
        }


        private object _locker = new object();
        private readonly IConfiguration _configuration;
        private IDictionary<string, IHttpRequestValidateable<JToken>>
                        _indexedValidators;
        public string AccessingConfigurationKey { get; set; } = "DefaultAccessing";
        public JTokenParametersValidateFilterAttribute(IConfiguration configuration)
        {
            _configuration = configuration;
            Initialize();
        }
        public virtual void Initialize()
        {
            LoadDynamicValidators();
        }
        protected virtual string[] GetDynamicValidatorsPathsProcess
                                    (
                                        //string dynamicLoadExecutorsPathsJsonFile = "dynamicCompostionPluginsPaths.json"
                                    )
        {
            var result = _configuration
                                .GetSection("DynamicValidatorsPaths")
                                .AsEnumerable()
                                .Select
                                    (
                                        (x) =>
                                        {
                                            return
                                                x.Value;
                                        }
                                    )
                                .ToArray();
            return result;
        }


        protected virtual void LoadDynamicValidators
                                (
                                   //string dynamicValidatorsPathsJsonFile = "dynamicValidatorsPaths.json"
                                )
        {
            var executingDirectory = Path
                                        .GetDirectoryName
                                                (
                                                    Assembly
                                                        .GetExecutingAssembly()
                                                        .Location
                                                );
            var validators = GetDynamicValidatorsPathsProcess
                                        (
                                            //dynamicValidatorsPathsJsonFile
                                        )
                                    .Select
                                        (
                                            (x) =>
                                            {
                                                var path = x;
                                                if (!path.IsNullOrEmptyOrWhiteSpace())
                                                {
                                                    if
                                                        (
                                                            x.StartsWith(".")
                                                        )
                                                    {
                                                        path = path.TrimStart('.', '\\', '/');
                                                    }
                                                    path = Path.Combine
                                                                    (
                                                                        executingDirectory
                                                                        , path
                                                                    );
                                                }
                                                return path;
                                            }
                                        )
                                    .Where
                                        (
                                            (x) =>
                                            {
                                                return
                                                    (
                                                        !x
                                                            .IsNullOrEmptyOrWhiteSpace()
                                                        &&
                                                        Directory
                                                            .Exists(x)
                                                    );
                                            }
                                        )
                                    .SelectMany
                                        (
                                            (x) =>
                                            {
                                                return
                                                    CompositionHelper
                                                            .ImportManyExportsComposeParts
                                                                <IHttpRequestValidateable<JToken>>
                                                                    (
                                                                        x
                                                                        , "*Validator*Plugin*.dll"
                                                                    );
                                            }
                                        );
            var indexedValidators = validators
                                        .Distinct
                                            (
                                                new FullTypeNameEqualityComparer<IHttpRequestValidateable<JToken>>()
                                            )
                                        .ToDictionary
                                            (
                                                (x) =>
                                                {
                                                    return
                                                        x.Name;
                                                }
                                                , StringComparer
                                                        .OrdinalIgnoreCase
                                            );
            _locker
                .LockIf
                    (
                        () =>
                        {
                            var r = (_indexedValidators == null);
                            return r;
                        }
                        , () =>
                        {
                            _indexedValidators = indexedValidators;
                        }
                    );
        }
        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var request = httpContext.Request;
            var httpMethod = $"http{request.Method}";
            var routeName = (string) context.ActionArguments["routeName"];

            var validatorConfiguration =
                    _configuration
                            .GetSection
                                ($"Routes:{routeName}:{httpMethod}:{AccessingConfigurationKey}:RequestValidator");
            if (validatorConfiguration.Exists())
            {
                var validatorName = validatorConfiguration.Value;
                var parameter = context
                                    .ActionArguments["parameters"] as JToken;
                var hasValidator = _indexedValidators
                                                .TryGetValue
                                                        (
                                                            validatorName
                                                            , out var validator
                                                        );
                IActionResult result;
                bool isValid;
                if (hasValidator)
                {
                    (isValid, result) = validator
                                                .Validate
                                                    (
                                                        parameter
                                                        , context
                                                    );
                }
                else
                {
                    isValid = false;
                    result = new JsonResult
                                    (
                                        new
                                        {
                                            statusCode = 400
                                            ,
                                            message = "can't validate"
                                        }
                                    )
                    {
                        StatusCode = 400
                        , ContentType = "application/json"
                    };
                }
                if (!isValid)
                {
                    context
                        .Result = result;
                }
            }
        }
        public virtual void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
#endif
