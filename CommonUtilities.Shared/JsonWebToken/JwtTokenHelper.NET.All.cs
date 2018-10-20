#if NETCOREAPP2_X
namespace Microshaoft
{
    using Microshaoft.WebApi;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Primitives;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    //using Microsoft.IdentityModel.Tokens;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text;
    using System.Web;

    //#if NETFRAMEWORK4_X
    //    using System.Web.Http.Controllers;
    //#endif

    public class JsonWebTokenUser : IIdentity
    {
        public JsonWebTokenUser
                    (
                        string name
                        , string authenticationType
                        , bool isAuthenticated
                    )
        {
            Name = name;
            AuthenticationType = authenticationType;
            IsAuthenticated = isAuthenticated;
        }
        public bool IsAuthenticated
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            private set;
        }
        public string AuthenticationType
        {
            get;
            private set;
        }
    }

    public static partial class JwtTokenHelper
    {
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
                                    |
                                    BindingFlags.Static
                                    |
                                    BindingFlags.FlattenHierarchy
                                )
                            .Where
                                (
                                    (x) =>
                                    {
                                        return
                                            (
                                                x.FieldType == typeof(string)
                                                &&
                                                x.IsLiteral
                                                &&
                                                !x.IsInitOnly
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

        public static bool TryValidateToken
                                (
                                    string plainTextSecurityKey
                                    , string token
                                    , out JwtSecurityToken validatedPlainToken
                                    , out ClaimsPrincipal claimsPrincipal
                                )
        {
            var r = false;
            validatedPlainToken = null;
            claimsPrincipal = null;
            try
            {
                var tokenHandler = new System
                                            .IdentityModel
                                            .Tokens
                                            .Jwt
                                            .JwtSecurityTokenHandler();
                var jwtSecurityToken = ((JwtSecurityToken)tokenHandler.ReadToken(token));
                var signingKey = new Microsoft
                                        .IdentityModel
                                        .Tokens
                                        .SymmetricSecurityKey
                                                (
                                                    Encoding
                                                        .UTF8
                                                        .GetBytes
                                                            (
                                                                plainTextSecurityKey
                                                            )
                                                );
                var tokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = jwtSecurityToken.Issuer,
                    ValidateIssuer = true,
                    ValidAudiences = jwtSecurityToken.Audiences,
                    ValidateAudience = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = false
                };

                claimsPrincipal = tokenHandler
                                        .ValidateToken
                                            (
                                                token
                                                , tokenValidationParameters
                                                , out var validatedPlainToken0
                                            );
                validatedPlainToken = validatedPlainToken0 as JwtSecurityToken;
                r = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return r;
        }
        public static bool TryIssueToken
                            (
                                string issuer
                                , string audience
                                , string userName
                                , JObject jClaimsIdentity
                                , string plainTextSecurityKey
                                , out Microsoft
                                            .IdentityModel
                                            .Tokens
                                            .SecurityToken plainToken
                                , out string secretTokenString
                                //, IIdentity identity = null
                                , string signingCredentialsAlgorithm
                                            = Microsoft
                                                .IdentityModel
                                                .Tokens
                                                .SecurityAlgorithms
                                                .HmacSha256Signature
                                , string signingCredentialsDigest
                                            = Microsoft
                                                .IdentityModel
                                                .Tokens
                                                .SecurityAlgorithms
                                                .Sha256Digest
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
                            , userName
                            , claims
                            , plainTextSecurityKey
                            , out plainToken
                            , out secretTokenString
                            //, identity
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
                                , string userName
                                , IEnumerable<Claim> claims
                                , string plainTextSecurityKey
                                , out Microsoft
                                        .IdentityModel
                                        .Tokens
                                        .SecurityToken plainToken
                                , out string secretTokenString
                                //, IIdentity identity = null
                                , string signingCredentialsAlgorithm
                                            = Microsoft
                                                .IdentityModel
                                                .Tokens
                                                .SecurityAlgorithms
                                                .HmacSha256Signature
                                , string signingCredentialsDigest
                                            = Microsoft
                                                .IdentityModel
                                                .Tokens
                                                .SecurityAlgorithms
                                                .Sha256Digest
                                , string plainTextSecurityKeyEncoding = "UTF-8"

                            )
        {
            bool r = false;
            plainToken = null;
            secretTokenString = null;
            try
            {
                var signingKey =
                            new Microsoft
                                    .IdentityModel
                                    .Tokens
                                    .SymmetricSecurityKey
                                            (
                                                Encoding
                                                    .GetEncoding
                                                        (
                                                            plainTextSecurityKeyEncoding
                                                        )
                                                    .GetBytes(plainTextSecurityKey)
                                            );
                var signingCredentials
                            = new Microsoft
                                        .IdentityModel
                                        .Tokens
                                        .SigningCredentials
                                                    (
                                                        signingKey
                                                        , signingCredentialsAlgorithm
                                                        , signingCredentialsDigest
                                                    );
                r = TryIssueToken
                         (
                            issuer
                            , audience
                            , userName
                            , claims
                            , out plainToken
                            , out secretTokenString
                            , signingKey
                            , signingCredentials
                         //, identity
                         );
            }
            catch //(Exception)
            {

                //throw;
            }
            return
                   r;
        }
        public static bool TryIssueToken
                    (
                        string issuer
                        , string audience
                        , string userName
                        , IEnumerable<Claim> claims
                        , out Microsoft
                                    .IdentityModel
                                    .Tokens
                                    .SecurityToken plainToken
                        , out string secretTokenString
                        , Microsoft
                                .IdentityModel
                                .Tokens
                                .SymmetricSecurityKey
                                                signingKey
                        , Microsoft
                                .IdentityModel
                                .Tokens
                                .SigningCredentials
                                                signingCredentials
                    //, IIdentity identity = null
                    )
        {
            bool r = false;
            plainToken = null;
            secretTokenString = null;
            try
            {
                IIdentity user = new JsonWebTokenUser
                                        (
                                            userName
                                            , "jwt"
                                            , true
                                        );
                var claimsIdentity = new ClaimsIdentity(user, claims);
                var securityTokenDescriptor =
                                    new Microsoft
                                            .IdentityModel
                                            .Tokens
                                            .SecurityTokenDescriptor()
                                    {
                                        Issuer = issuer,
                                        Audience = audience,
                                        IssuedAt = DateTime.Now,
                                        Subject = claimsIdentity,
                                        SigningCredentials = signingCredentials,
                                    };
                var tokenHandler = new System
                                            .IdentityModel
                                            .Tokens
                                            .Jwt
                                            .JwtSecurityTokenHandler();
                plainToken = tokenHandler.CreateToken(securityTokenDescriptor);
                secretTokenString = tokenHandler.WriteToken(plainToken);
                r = true;
            }
            catch// (Exception e)
            {
                //throw;
            }
            return
                   r;
        }

        public static bool TryParseJTokenParameters
                            (
                                HttpRequest request
                                , out JToken parameters
                                , out string secretJwtToken
                                , Action<JToken> onFormProcessAction = null
                                , string jwtTokenName = "xJwtToken"
                            )
        {
            var r = false;
            parameters = null;
            secretJwtToken = string.Empty;

            JToken jToken = null;
            async void RequestFormBodyProcess()
            {
                if (request.HasFormContentType)
                {
                    onFormProcessAction?.Invoke(jToken);
                }
                else
                {
                    //if (request.IsJsonRequest())
                    {
                        using (var streamReader = new StreamReader(request.Body))
                        {
                            var task = streamReader.ReadToEndAsync();
                            await task;
                            var json = task.Result;
                            if (!json.IsNullOrEmptyOrWhiteSpace())
                            {
                                jToken = JToken.Parse(json);
                            }
                        }
                    }
                }
            }
            void RequestQueryStringHeaderProcess()
            {
                var qs = request.QueryString.Value;
                if (qs.IsNullOrEmptyOrWhiteSpace())
                {
                    return;
                }
                qs = HttpUtility
                            .UrlDecode
                                (
                                    qs
                                );
                if (qs.IsNullOrEmptyOrWhiteSpace())
                {
                    return;
                }
                qs = qs.TrimStart('?');
                if (qs.IsNullOrEmptyOrWhiteSpace())
                {
                    return;
                }
                var isJson = false;
                try
                {
                    jToken = JToken.Parse(qs);
                    isJson = jToken is JObject;
                }
                catch
                {

                }
                if (!isJson)
                {
                    jToken = request.Query.ToJToken();
                }
            }
            // 取 jwtToken 优先级顺序：Header → QueryString → Body
            StringValues jwtToken = string.Empty;
            var needExtractJwtToken = !jwtTokenName.IsNullOrEmptyOrWhiteSpace();
            void ExtractJwtTokenInJToken()
            {
                if (needExtractJwtToken)
                {
                    if (jToken != null)
                    {
                        if (StringValues.IsNullOrEmpty(jwtToken))
                        {
                            var j = jToken[jwtTokenName];
                            if (j != null)
                            {
                                jwtToken = j.Value<string>();
                            }
                        }
                    }
                }
            }
            if (needExtractJwtToken)
            {
                request
                    .Headers
                    .TryGetValue
                        (
                           jwtTokenName
                           , out jwtToken
                        );
            }
            RequestQueryStringHeaderProcess();
            ExtractJwtTokenInJToken();
            if
                (
                    string.Compare(request.Method, "post", true) == 0
                )
            {
                RequestFormBodyProcess();
                ExtractJwtTokenInJToken();
                //if (jToken == null)
                //{
                //    RequestHeaderProcess();
                //}
            }
            parameters = jToken;
            secretJwtToken = jwtToken;
            r = true;
            return r;
        }
        

    }

    public static partial class HttpRequestExtensionsManager
    {
        public static bool TryParseJTokenParameters
                            (
                                this HttpRequest request
                                , out JToken parameters
                                , out string secretJwtToken
                                , Action<JToken> onFormProcessAction = null
                                , string jwtTokenName = "xJwtToken"
                            )
        {
            return
                JwtTokenHelper
                    .TryParseJTokenParameters
                        (
                            request
                            , out parameters
                            , out secretJwtToken
                            , onFormProcessAction
                            , jwtTokenName
                        );

        }


    }

}
#endif