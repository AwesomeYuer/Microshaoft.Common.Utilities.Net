#if !XAMARIN
namespace Microshaoft
{
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IdentityModel.Tokens.Jwt;
    //using Microsoft.IdentityModel.Tokens;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Claims;
    using System.Text;
    using System.Web;
//#if NETFRAMEWORK4_X
//    using System.Web.Http.Controllers;
//#endif
    public static partial class JwtTokenHelper
    {

        private static string _webTokenPlainTextSecretKey
                = //"ABCDEFGHABCDEFGHABCDEFGHABCDEFGHABCDEFGH";
                    ConfigurationManager
                        .AppSettings["WebTokenSecretKey"];
        private static string[] _webTokenIssuers = new string[] { "SOPS" };
        private static string[] _webTokenAudiences = new string[] { "SOPS" };

        private static int _webTokenExpireInSeconds
                = int.Parse
                        (
                            ConfigurationManager
                                .AppSettings["WebTokenExpiredInSeconds"]
                        );


        private static string _ssoTokenPlainTextSecretKey 
                = //"ZBCDEFGHZBCDEFGHZBCDEFGHZBCDEFGHZBCDEFGH";
                    ConfigurationManager
                        .AppSettings["SsoTokenSecretKey"];
        private static string[] _ssoTokenIssuers = new string[] { "BL" };
        private static string[] _ssoTokenAudiences = new string[] { "SOPS" };
        private static int _ssoTokenExpireInSeconds
                = int.Parse
                        (
                            ConfigurationManager
                                .AppSettings["SsoTokenExpiredInSeconds"]
                        );

        private static IDictionary<string, string> _claimTypes
            = new Func<IDictionary<string, string>>
                (
                    () =>
                    {
                        return
                        typeof(ClaimTypes)
                            .GetFields
                                (
                                    BindingFlags.Public
                                    | BindingFlags.Static
                                    | BindingFlags.FlattenHierarchy
                                )
                            .Where
                                (
                                    (x) =>
                                    {
                                        return
                                            (
                                                x.FieldType == typeof(string)
                                                && x.IsLiteral
                                                && !x.IsInitOnly
                                            );
                                    }
                                )
                            .ToDictionary
                                (
                                    (x) =>
                                    {
                                        return
                                            x.Name;
                                    }
                                    , (x) =>
                                    {
                                        return
                                            x.GetValue(null).ToString();
                                    }
                                    , StringComparer.OrdinalIgnoreCase
                                );


                    }
                )();


        public static DateTime ParseSecondsToLocalTime(double seconds)
        {
            return new DateTime(1970, 1, 1).AddSeconds(seconds).ToLocalTime();
        }

        public static string GetClaimTypeValue(this ClaimsPrincipal target, string claimType)
        {
            var r = target
                        .Claims
                        .FirstOrDefault
                            (
                                (x) =>
                                {
                                    return
                                        (x.Type == claimType);
                                }
                            ).Value;
            return r;
        }

        public static DateTime GetIssuedAtTime(this ClaimsPrincipal target)
        {
            var r =
                    ParseSecondsToLocalTime
                        (
                            double.Parse(GetClaimTypeValue(target, "iat"))
                        );

            return r;
        }
        public static bool TryValidateSsoToken
                        (
                            string token
                            , out ClaimsPrincipal claimsPrincipal
                            , string ip = null
                            //, int expiredInSeconds = -1
                        )
        {
            var r = false;
            var plainTextSecurityKey = _ssoTokenPlainTextSecretKey;
            var issuers = _ssoTokenIssuers;
            var audiences = _ssoTokenAudiences;
            SecurityToken validatedToken = null;
            r = TryValidateToken
                        (
                            plainTextSecurityKey
                            , token
                            , issuers
                            , audiences
                            , out validatedToken
                            , out claimsPrincipal
                        );
            if (r)
            {
                IPAddress ipa;
                if (IPAddress.TryParse(ip, out ipa))
                {
                    r = (claimsPrincipal.GetClaimTypeValue("UserClientIP") == ip);
                }
            }
            if (r)
            {
                if (_ssoTokenExpireInSeconds >= 0)
                {
                    var issuedAt = claimsPrincipal.GetIssuedAtTime();
                    r = (Math.Abs(DateTimeHelper.SecondsDiffNow(issuedAt)) < _ssoTokenExpireInSeconds);
                }
            }
            if (!r)
            {
                claimsPrincipal = null;
            }
            return r;
        }
        public static bool TryValidateToken
                                (
                                    string plainTextSecurityKey
                                    , string token
                                    , string[] issuers
                                    , string[] audiences
                                    , out Microsoft.IdentityModel.Tokens.SecurityToken validatedToken
                                    , out ClaimsPrincipal claimsPrincipal
                                )
        {
            var r = false;
            validatedToken = null;
            claimsPrincipal = null;
            try
            {

                var signingKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(plainTextSecurityKey));
                var tokenValidationParameters = new TokenValidationParameters()
                {
                    ValidAudiences = audiences,
                    ValidIssuers = issuers,
                    IssuerSigningKey = signingKey
                };
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jst = ((JwtSecurityToken)tokenHandler.ReadToken(token));
                claimsPrincipal = tokenHandler
                            .ValidateToken
                                (
                                    token
                                    , tokenValidationParameters
                                    , out validatedToken
                                );
                r = true;
            }
            catch (Exception e)
            {

                Console.WriteLine();
            }
            return r;
        }


        public static bool TryIssueWebToken
                       (
                           IEnumerable<Claim> claims
                           , out string secretTokenString
                          
                       )
        {
            SecurityToken plainToken;
            var r = TryIssueToken
                        (
                            _webTokenIssuers[0]
                            , _webTokenAudiences[0]
                            , claims
                            , _webTokenPlainTextSecretKey
                            , out plainToken
                            , out secretTokenString
                        );
            return r;
        }
        public static bool TryIssueSsoToken
                       (
                           JObject jClaimsIdentity
                           , out string secretTokenString

                       )
        {
            SecurityToken plainToken;
            var r = TryIssueToken
                        (
                            _ssoTokenIssuers[0]
                            , _ssoTokenAudiences[0]
                            , jClaimsIdentity
                            , _ssoTokenPlainTextSecretKey
                            , out plainToken
                            , out secretTokenString
                        );
            return r;
        }
        public static bool TryIssueToken
                            (
                                string issuer
                                , string audience
                                , JObject jClaimsIdentity
                                , string plainTextSecurityKey
                                , out Microsoft.IdentityModel.Tokens.SecurityToken plainToken
                                , out string secretTokenString
                                , string signingCredentialsAlgorithm = Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature
                                , string signingCredentialsDigest = Microsoft.IdentityModel.Tokens.SecurityAlgorithms.Sha256Digest
                                , string plainTextSecurityKeyEncoding = "UTF-8"
                            )
        {
            var jValues = jClaimsIdentity
                                     .GetAllJValues();
            var claims = jValues
                            .Select
                                (
                                    (x) =>
                                    {
                                        var value = string.Empty;
                                        var key = x.Path;
                                        if (_claimTypes.TryGetValue(key, out value))
                                        {
                                            key = value;
                                        }
                                        else
                                        {
                                            key = x.Path;
                                        }
                                        return
                                            new Claim
                                                    (
                                                        key
                                                        , x.Value.ToString()
                                                    );
                                    }
                                );
            var r = TryIssueToken
                        (
                            issuer
                            , audience
                            , claims
                            , plainTextSecurityKey
                            , out plainToken
                            , out secretTokenString
                            , signingCredentialsAlgorithm
                            , signingCredentialsDigest
                            , plainTextSecurityKeyEncoding
                        );
            return r;
        }

        public static bool TryIssueToken
                            (
                                string issuer
                                , string audience
                                , IEnumerable<Claim> claims
                                , string plainTextSecurityKey
                                , out Microsoft.IdentityModel.Tokens.SecurityToken plainToken
                                , out string secretTokenString
                                , string signingCredentialsAlgorithm = Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature
                                , string signingCredentialsDigest = Microsoft.IdentityModel.Tokens.SecurityAlgorithms.Sha256Digest
                                , string plainTextSecurityKeyEncoding = "UTF-8"
                            )
        {
            bool r = false;
            plainToken = null;
            secretTokenString = null;
            try
            {
                var signingKey = new Microsoft
                                           .IdentityModel
                                           .Tokens
                                           .SymmetricSecurityKey
                                                   (
                                                       Encoding
                                                           .GetEncoding(plainTextSecurityKeyEncoding)
                                                           .GetBytes(plainTextSecurityKey)
                                                   );
                var signingCredentials
                            = new Microsoft.IdentityModel.Tokens.SigningCredentials
                                (
                                    signingKey
                                    , signingCredentialsAlgorithm
                                    , signingCredentialsDigest
                                );
                // ComplexClaim
                var claimsIdentity = new ClaimsIdentity
                (
                    claims
                    , "Custom"
                );
                var securityTokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor()
                {
                    Issuer = issuer,
                    Audience = audience,
                    IssuedAt = DateTime.Now,
                    Subject = claimsIdentity,
                    SigningCredentials = signingCredentials,
                };

                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                plainToken = tokenHandler.CreateToken(securityTokenDescriptor);
                secretTokenString = tokenHandler.WriteToken(plainToken);
                r = true;
            }
            catch //(Exception)
            {

                //throw;
            }
            return
                   r;
        }

        public static bool TryGetCurrentWebUser
                                    (
                                        string token
                                        , out ClaimsPrincipal claimsPrincipal
                                        , string ip = null
                                        , int expiredInSeconds = -1
                                    )
        {
            var r = false;
            claimsPrincipal = null;
            
            SecurityToken validatedToken;
            r = TryValidateToken
                    (
                        _webTokenPlainTextSecretKey
                        , token
                        , _webTokenIssuers
                        , _webTokenAudiences
                        , out validatedToken
                        , out claimsPrincipal
                    );
            IPAddress ipa = null;
            if (IPAddress.TryParse(ip, out ipa))
            {
                var tokenIP = claimsPrincipal.GetClaimTypeValue("UserClientIP");
                if (IPAddress.TryParse(tokenIP, out ipa))
                {
                    r = (tokenIP == ip);
                }
            }
            if (r)
            {
                if (expiredInSeconds >= 0)
                {
                    var issuedAt = claimsPrincipal.GetIssuedAtTime();
                    r = (Math.Abs(DateTimeHelper.SecondsDiffNow(issuedAt)) < expiredInSeconds);
                }
            }
            if (!r)
            {
                claimsPrincipal = null;
            }
            return r;
        }

    }
}
#endif