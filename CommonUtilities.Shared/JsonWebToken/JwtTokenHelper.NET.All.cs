#if !XAMARIN
namespace Microshaoft
{
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    //using Microsoft.IdentityModel.Tokens;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text;
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
    }
}
#endif