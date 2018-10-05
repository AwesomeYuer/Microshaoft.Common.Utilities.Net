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

    public class BearerTokenBasedAuthorizeWebApiFilter
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

        private string _jwtName;
        private TokenStoreFlags _jwtCarrier;
        private string _jwtIssuer;
        private string[] _jwtAudiences;
        private bool _jwtNeedValidIP = false;
        private string _jwtSecretKey;
        private int _jwtExpireInSeconds = 0;

        public BearerTokenBasedAuthorizeWebApiFilter()
        {
            Initialize();
        }
        public virtual void Initialize()
        {
            InstanceID = Interlocked.Increment(ref InstancesSeed);
            //允许继承覆盖, 构造函数
            LoadConfiguration();
        }
        public virtual void LoadConfiguration
                                (
                                    string jwtValidationJsonFile = "JwtValidation.json"
                                )
        {
            var configurationBuilder =
                        new ConfigurationBuilder()
                                .AddJsonFile(jwtValidationJsonFile);
            var configuration = configurationBuilder.Build();
            _jwtName = configuration
                            .GetSection("TokenName")
                            .Value;
            _jwtCarrier = Enum
                            .Parse<TokenStoreFlags>
                                (
                                    configuration
                                        .GetSection("TokenCarrier")
                                        .Value
                                    , true
                                );
            _jwtIssuer = configuration
                                .GetSection("Issuer")
                                .Value;
            _jwtAudiences = configuration
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
            _jwtNeedValidIP = bool
                                .Parse
                                    (
                                        configuration
                                            .GetSection("NeedValidIP")
                                            .Value
                                    );
            _jwtSecretKey = configuration
                                .GetSection("SecretKey")
                                .Value;
            _jwtExpireInSeconds = int
                                    .Parse
                                        (
                                            configuration
                                                .GetSection("ExpireInSeconds")
                                                .Value
                                        );
        }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            StringValues token = string.Empty;
            var ok = false;
            if (_jwtCarrier.HasFlag(TokenStoreFlags.Header))
            {
                ok = request.Headers.TryGetValue(_jwtName, out token);
            }
            else if (_jwtCarrier.HasFlag(TokenStoreFlags.Cookie))
            {
                ok = request.Cookies.TryGetValue(_jwtName, out var t);
                token = t;
            }
            if (ok)
            {
                ok = JwtTokenHelper
                            .TryValidateToken
                                (
                                    _jwtSecretKey
                                    , token
                                    , out var validatedPlainToken
                                    , out var claimsPrincipal
                                );
                if (ok)
                {
                    if (_jwtExpireInSeconds > 0)
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
                                    _jwtExpireInSeconds
                                )
                            );
                    }
                }
                if (ok)
                {
                    ok = (string.Compare(validatedPlainToken.Issuer, _jwtIssuer, true) == 0);
                }
                if (ok)
                {
                    ok = _jwtAudiences
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
                }
                if (ok)
                {
                    var userName1 = context.HttpContext.User.Identity.Name;
                    var userName2 = claimsPrincipal.Identity.Name;
                    ok = (string.Compare(userName1, userName2, true) == 0);
                }
                if (ok)
                {
                    if (_jwtNeedValidIP)
                    {
                        var requestIpAddress =
                                            context
                                                .HttpContext
                                                .Connection
                                                .RemoteIpAddress;
                        var tokenIpAddress = claimsPrincipal.GetClientIP();
                        ok = (requestIpAddress.ToString() == tokenIpAddress.ToString());
                    }
                }
                if (ok)
                {
                    context.HttpContext.User = claimsPrincipal;
                }
            }
            if (!ok)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
        public virtual void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
#endif