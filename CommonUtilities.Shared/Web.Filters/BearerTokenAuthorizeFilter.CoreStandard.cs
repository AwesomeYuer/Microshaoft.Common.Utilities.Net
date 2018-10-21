#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Linq;
    using System.Threading;
    [Flags]
    public enum TokenStoreFlags : ushort
    {
        Header = 0b0000_00001
        , Cookie = 0b0000_00010
    }

    public class BearerTokenBasedAuthorizeFilter
                    :
                        //AuthorizeAttribute
                        Attribute
                        , IActionFilter
    {
        public static int InstancesSeed = 0;
        public int InstanceID
        {
            private set;
            get;
        }

        public BearerTokenBasedAuthorizeFilter()
        {
            Initialize();
        }
        public virtual void Initialize()
        {
            InstanceID = Interlocked.Increment(ref InstancesSeed);
        }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            var ok = false;
            var errorMessage = string.Empty;
            var errorStatusCode = -1;
            void ErrorResult()
            {
                //context.Result = new ForbidResult();
                context.Result = new ContentResult()
                {
                    StatusCode = errorStatusCode
                     ,
                    ContentType = "application/json"
                     ,
                    Content =
$@"{{
    StatusCode : {errorStatusCode}
    , Message : ""{errorMessage}""
}}"
                };
                //return;
            }

            var request = context.HttpContext.Request;
            StringValues jwtToken = string.Empty;
            
            IConfiguration configuration =
                        (IConfiguration)context
                                            .HttpContext
                                            .RequestServices
                                            .GetService
                                                (
                                                    typeof(IConfiguration)
                                                );
            var jwtTokenName = configuration
                                    .GetSection("TokenName")
                                    .Value;
            if
                (
                    context
                        .HttpContext
                        .Items
                        .TryGetValue
                            (
                                jwtTokenName
                                , out object value
                            )
                )
            {
                jwtToken = value.ToString();
                ok = true;
            }
            else
            {
                ok = request
                        .TryParseJTokenParameters
                            (
                                out var parameters
                                , out var secretJwtToken
                                , null
                                , jwtTokenName
                            );
                ok = !StringValues.IsNullOrEmpty(secretJwtToken);
                if (ok)
                {
                    jwtToken = secretJwtToken;
                }
            }
            if (!ok)
            {
                errorStatusCode = 400;
                errorMessage = "Bad Request, Jwt not found";
                ErrorResult();
                return;
            }
            if (ok)
            {
                var jwtSecretKey = configuration
                                        .GetSection("SecretKey")
                                        .Value;
                ok = JwtTokenHelper
                            .TryValidateToken
                                (
                                    jwtSecretKey
                                    , jwtToken
                                    , out var validatedPlainToken
                                    , out var claimsPrincipal
                                );
                if (!ok)
                {
                    errorStatusCode = 400;
                    errorMessage = "Bad Request, Jwt Invalidate";
                    ErrorResult();
                    return;
                }
                if (ok)
                {
                    var jwtExpireInSeconds =
                                int
                                    .Parse
                                        (
                                            configuration
                                                .GetSection("ExpireInSeconds")
                                                .Value
                                        );
                    if (jwtExpireInSeconds > 0)
                    {
                        var iat = claimsPrincipal
                                        .GetIssuedAtLocalTime();
                        var diffNowSeconds = DateTimeHelper
                                                .SecondsDiffNow(iat.Value);
                        ok =
                            (
                                (
                                    diffNowSeconds
                                    >=
                                    0
                                )
                                &&
                                (
                                    diffNowSeconds
                                    <=
                                    jwtExpireInSeconds
                                )
                            );
                        if (!ok)
                        {
                            errorStatusCode = 403;
                            errorMessage = "Forbid Request, Jwt expired";
                            ErrorResult();
                            return;
                        }
                    }
                }
                if (ok)
                {
                    var jwtIssuer = configuration
                                        .GetSection("Issuer")
                                        .Value;
                    ok =
                        (
                            string
                                .Compare
                                    (
                                        validatedPlainToken.Issuer
                                        , jwtIssuer
                                        , true
                                    )
                            ==
                            0
                        );
                    if (!ok)
                    {
                        errorStatusCode = 400;
                        errorMessage = "Bad Request, Jwt Invalidate Issuer";
                        ErrorResult();
                        return;
                    }
                }
                if (ok)
                {
                    var jwtAudiences = configuration
                                            .GetSection("Audiences")
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
                    ok = jwtAudiences
                             .Any
                                 (
                                     (x) =>
                                     {
                                         return
                                             validatedPlainToken
                                                .Audiences
                                                .Any
                                                    (
                                                        (xx) =>
                                                        {
                                                            return
                                                                (xx == x);
                                                        }
                                                    );
                                     }
                                 );
                    if (!ok)
                    {
                        errorStatusCode = 403;
                        errorMessage = "Forbid Request, Jwt Invalidate Audiences";
                        ErrorResult();
                        return;
                    }
                }
                if (ok)
                {
                    var jwtNeedValidUserName =
                                bool
                                    .Parse
                                        (
                                            configuration
                                                    .GetSection("NeedValidUserName")
                                                    .Value
                                        );
                    if (jwtNeedValidUserName)
                    {
                        var userName1 = context
                                            .HttpContext
                                            .User
                                            .Identity
                                            .Name;
                        var userName2 = claimsPrincipal
                                            .Identity
                                            .Name;
                        ok =
                            (
                                string
                                    .Compare
                                        (
                                            userName1
                                            , userName2
                                            , true
                                        )
                                ==
                                0
                            );
                        if (!ok)
                        {
                            errorStatusCode = 403;
                            errorMessage = "Bad Request, Jwt Invalidate userName";
                            ErrorResult();
                            return;
                        }
                    }
                }
                if (ok)
                {
                    var jwtNeedValidIP = bool
                                            .Parse
                                                (
                                                    configuration
                                                        .GetSection("NeedValidIP")
                                                        .Value
                                                );
                    if (jwtNeedValidIP)
                    {
                        var requestIpAddress =
                                            context
                                                .HttpContext
                                                .Connection
                                                .RemoteIpAddress;
                        var tokenIpAddress = claimsPrincipal.GetClientIP();
                        ok =
                            (
                                requestIpAddress.ToString()
                                ==
                                tokenIpAddress.ToString()
                            );
                        if (!ok)
                        {
                            errorStatusCode = 400;
                            errorMessage = "Bad Request, Jwt Invalidate IP";
                            ErrorResult();
                            return;
                        }
                    }
                }
                if (ok)
                {
                    context
                        .HttpContext
                        .User = claimsPrincipal;
                }
            }
            if (!ok)
            {
                ErrorResult();
                return;
            }
        }
        public virtual void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
#endif