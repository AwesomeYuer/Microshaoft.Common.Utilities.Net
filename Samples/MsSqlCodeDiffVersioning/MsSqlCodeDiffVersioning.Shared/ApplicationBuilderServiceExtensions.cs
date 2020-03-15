namespace WebApplication.ASPNetCore
{
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Swashbuckle.AspNetCore.Filters;
    public static class ApplicationBuilderServiceExtensions
    {
        private const string swaggerVersion = "v3.1.101";
        private const string swaggerTitle = "Microshaoft Store Procedures Executors API";
        private const string swaggerDescription = "Powered By Microshaoft.Common.Utilities.Net";
        // https://thecodebuzz.com/jwt-authorization-token-swagger-open-api-asp-net-core-3-0/
        public static void AddSwaggerGenDefault(this IServiceCollection target)
        {
            target
                .AddSwaggerGen
                    (
                        (c) =>
                        {
                            c
                                .SwaggerDoc
                                    (
                                        swaggerVersion
                                        , new OpenApiInfo
                                        {
                                            Version = swaggerVersion
                                                ,
                                            Title = swaggerTitle
                                                ,
                                            Description = swaggerDescription
                                                ,
                                            TermsOfService = new Uri("https://github.com/Microshaoft/Microshaoft.Common.Utilities.Net/blob/master/README.md")
                                                ,
                                            Contact = new OpenApiContact
                                            {
                                                Name = "Microshaoft"
                                                        ,
                                                Email = "Microshaoft@gmail.com"
                                                        ,
                                                Url = new Uri("https://github.com/Microshaoft")
                                                        ,
                                            }
                                                ,
                                            License = new OpenApiLicense
                                            {
                                                Name = "Use under License"
                                                        ,
                                                Url = new Uri("https://github.com/Microshaoft/Microshaoft.Common.Utilities.Net/blob/master/License.txt")
                                                        ,
                                            }
                                        }
                                    );
                            c
                                .OperationFilter<SwaggerCustomHeadersFilter>();
                            
                            c
                                .AddSecurityDefinition
                                    (
                                        "Bearer"
                                        , new OpenApiSecurityScheme
                                        {
                                            Name = "Authorization",
                                            Type = SecuritySchemeType.ApiKey,
                                            Scheme = "Bearer",
                                            BearerFormat = "JWT",
                                            In = ParameterLocation.Header,
                                            Description = "JWT Authorization header using the Bearer scheme.",
                                        }
                                    );

                            c
                                .AddSecurityRequirement
                                    (
                                        new OpenApiSecurityRequirement
                                        {
                                            {
                                                  new OpenApiSecurityScheme
                                                    {
                                                        Reference = new OpenApiReference
                                                        {
                                                            Type = ReferenceType.SecurityScheme,
                                                            Id = "Bearer",
                                                        }
                                                    },
                                                    new string[] {}
                                            }
                                        }
                                    );
                            // Set the comments path for the Swagger JSON and UI.
                            //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                            //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                            //c.IncludeXmlComments(xmlPath);
                            c
                                .ResolveConflictingActions
                                    (
                                        (xApiDescriptions) =>
                                        {

                                            foreach (var apiDescription in xApiDescriptions)
                                            {
                                                Console.WriteLine(apiDescription);
                                            
                                            }


                                            return
                                                xApiDescriptions
                                                            .First();
                                        }
                                    );
                            //c.UseInlineDefinitionsForEnums();
                            
                            c.MapType<JToken>
                                (
                                    () =>
                                    {
                                        return
                                            new OpenApiSchema { Type = "object", AdditionalPropertiesAllowed = true };
                                    }
                                );

                        }
                    );

        }

        public static IApplicationBuilder UseSwaggerDefault(this IApplicationBuilder target)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            target
                .UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            target
                .UseSwaggerUI
                    (
                        (c) =>
                        {
                            c
                                .SwaggerEndpoint
                                    (
                                        $"/swagger/{swaggerVersion}/swagger.json"
                                        , swaggerTitle
                                    );
                        }
                    );
            return
                target;
        }
    }
    public class SwaggerCustomHeadersFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            //var parameters = operation.Parameters;
        

            //if (apiDescription.HttpMethod == "POST")
            //{
            //    //var para = operation.Parameters.ToArray();
            //    foreach (var parameter in parameters)
            //    {
            //        if (parameter.Name == "parameters")
            //        {
            //            foreach (var content in operation.RequestBody.Content)
            //            {
            //                content.add
            //            }
            //        }
            //    }
            //}

            //operation.




            operation
                .Parameters
                .Add
                    (
                        new OpenApiParameter
                        {
                            Name = "Sample-Header",
                            In = ParameterLocation.Header,
                            Required = false // set to false if this is optional
                        }
                    );
        }
    }

    public class Csv<T> : List<T> where T : IConvertible
    {
        public Csv<T> Append(string delimitedValues)
        {
            var splitValues = delimitedValues
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Cast<string>();

            var convertedValues = splitValues
                .Select(str => Convert.ChangeType(str, typeof(T)))
                .Cast<T>();

            this.AddRange(convertedValues);

            return this;
        }

        public override string ToString()
        {
            return this.Aggregate("", (a, s) => $"{a},{s}").Trim(',');
        }
    }


}
