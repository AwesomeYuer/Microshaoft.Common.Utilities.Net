#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;

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
        private const string _itemKeyOfRequestJTokenParameters = "requestJTokenParameters";
        private bool _isRequired = true;
        public bool IsRequired
        {
            set
            {
                _isRequired = value;
            }
            get
            {
                return _isRequired;
            }
        }

        public BearerTokenBasedAuthorizeFilter()
        {
            Initialize();
        }
        public virtual void Initialize()
        {
            //InstanceID = Interlocked.Increment(ref InstancesSeed);
        }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            var ok = false;
            var errorMessage = string.Empty;
            var errorStatusCode = -1;
            
            void ErrorResult()
            {
                context
                    .Result = new JsonResult
                                    (
                                        new
                                        {
                                            statusCode = errorStatusCode
                                            , message = errorMessage
                                        }
                                    )
                                {
                                    StatusCode = errorStatusCode
                                    , ContentType = "application/json"
                                };
            }
            var httpContext = context.HttpContext;
            var request = httpContext.Request;

            StringValues jwtToken = string.Empty;
            IConfiguration configuration =
                        (IConfiguration) context
                                                .HttpContext
                                                .RequestServices
                                                .GetService
                                                    (
                                                        typeof(IConfiguration)
                                                    );
            var jwtTokenName = "xJwtToken";
            var configurationTokenName = configuration.GetSection("TokenName");
            if 
                (
                    configurationTokenName.Exists()
                )
            {
                jwtTokenName = configurationTokenName.Value;
            }
            JToken parameters = null;
            string secretJwtToken = string.Empty;
            if
                (
                    httpContext
                        .Items
                        .TryGetValue
                            (
                                _itemKeyOfRequestJTokenParameters
                                , out object o
                            )
                )
            {
                parameters = (JToken) o;
            }
            else
            {
                ok = request
                        .TryParseJTokenParameters
                            (
                                out parameters
                                , out secretJwtToken
                                , null
                                , jwtTokenName
                            );
                if (ok)
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
            if
                (
                    httpContext
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
                ok = false;
                if (IsRequired)
                {
                    ErrorResult();
                    return;
                }
            }
            if (ok)
            {
                //JwtSecurityToken validatedPlainToken = null;
                //ClaimsPrincipal claimsPrincipal = null;
                var configurationSecretKey = configuration.GetSection("SecretKey");
                if 
                    (
                        configurationSecretKey.Exists()
                        ||
                        IsRequired
                    )
                {
                    var jwtSecretKey = configurationSecretKey.Value;
                    ok = JwtTokenHelper
                                .TryValidateToken
                                    (
                                        jwtToken
                                        , jwtSecretKey
                                        , out var validatedPlainToken
                                        , out var claimsPrincipal
                                    );
                    if (!ok)
                    {
                        errorStatusCode = 400;
                        errorMessage = "Bad Request, Jwt Invalidate";
                        ok = false;
                        if (IsRequired)
                        {
                            ErrorResult();
                            return;
                        }
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
                                ok = false;
                                if (IsRequired)
                                {
                                    ErrorResult();
                                    return;
                                }
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
                            ok = false;
                            if (IsRequired)
                            {
                                ErrorResult();
                                return;
                            }
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
                                                    );
                        //.ToArray();
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
                            ok = false;
                            if (IsRequired)
                            {
                                ErrorResult();
                                return;
                            }
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
                                ok = false;
                                if (IsRequired)
                                {
                                    ErrorResult();
                                    return;
                                }
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
                                ok = false;
                                if (IsRequired)
                                {
                                    ErrorResult();
                                    return;
                                }
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
            }
            if (!ok)
            {
                if (IsRequired)
                {
                    ErrorResult();
                    return;
                }
            }
        }
        public virtual void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
#endif