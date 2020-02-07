#if !XAMARIN
namespace Microshaoft
{
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text;

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


        public static bool TryValidateToken
                                (
                                    string secretToken
                                    , string plainTextSecurityKey
                                    , out JwtSecurityToken validatedPlainToken
                                    , out ClaimsPrincipal claimsPrincipal
                                    , bool validateLifetime = false
                                    , int clockSkewInSeconds = 300
                                )
        {
            var signingKey = new SymmetricSecurityKey
                                            (
                                                Encoding
                                                    .UTF8
                                                    .GetBytes
                                                        (
                                                            plainTextSecurityKey
                                                        )
                                            );

            var r = TryValidateToken
                        (
                            secretToken
                            , signingKey
                            , out validatedPlainToken
                            , out claimsPrincipal
                            , validateLifetime
                            , clockSkewInSeconds
                        );
            return r;
        }
        public static bool TryValidateToken
                        (
                            string secretToken
                            , SecurityKey signingKey
                            , out JwtSecurityToken validatedPlainToken
                            , out ClaimsPrincipal claimsPrincipal
                            , bool validateLifetime = false
                            , int clockSkewInSeconds = 300
                        )
        {
            var r = false;
            validatedPlainToken = null;
            claimsPrincipal = null;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = ((JwtSecurityToken) tokenHandler.ReadToken(secretToken));
                
                var tokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = jwtSecurityToken.Issuer,
                    ValidateIssuer = true,
                    ValidAudiences = jwtSecurityToken.Audiences,
                    ValidateAudience = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = validateLifetime,
                    ClockSkew = TimeSpan.FromSeconds(clockSkewInSeconds)
                };

                claimsPrincipal = tokenHandler
                                        .ValidateToken
                                            (
                                                secretToken
                                                , tokenValidationParameters
                                                , out var validatedPlainToken0
                                            );
                validatedPlainToken = validatedPlainToken0 as JwtSecurityToken;
                r = true;
            }
            catch //(Exception e)
            {
               // Console.WriteLine(e);
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
                                , out SecurityToken plainToken
                                , out string secretTokenString
                                //, IIdentity identity = null
                                , DateTime? notBefore = null
                                , DateTime? expires = null
                                , string signingCredentialsAlgorithm
                                            = SecurityAlgorithms
                                                .HmacSha256Signature
                                , string signingCredentialsDigest
                                            = SecurityAlgorithms
                                                .Sha256Digest
                                , string plainTextSecurityKeyEncoding = "UTF-8"
                            )
        {
            var claims = jClaimsIdentity.AsClaims();
            var r = TryIssueToken
                        (
                            issuer
                            , audience
                            , userName
                            , claims
                            , plainTextSecurityKey
                            , out plainToken
                            , out secretTokenString
                            , notBefore
                            , expires
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
                                , out SecurityToken plainToken
                                , out string secretTokenString
                                //, IIdentity identity = null
                                , DateTime? notBefore = null
                                , DateTime? expires = null
                                , string signingCredentialsAlgorithm
                                            = SecurityAlgorithms
                                                .HmacSha256Signature
                                , string signingCredentialsDigest
                                            = SecurityAlgorithms
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
                            new SymmetricSecurityKey
                                            (
                                                Encoding
                                                    .GetEncoding
                                                        (
                                                            plainTextSecurityKeyEncoding
                                                        )
                                                    .GetBytes(plainTextSecurityKey)
                                            );
                var signingCredentials
                            = new SigningCredentials
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
                            , notBefore
                            , expires
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
                        , out SecurityToken plainToken
                        , out string secretTokenString
                        , SecurityKey
                                        signingKey
                        , SigningCredentials
                                            signingCredentials
                        , DateTime? notBefore = null
                        , DateTime? expires = null
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
                                    new SecurityTokenDescriptor()
                                    {
                                        Issuer = issuer,
                                        Audience = audience,
                                        IssuedAt = DateTime.Now,
                                        Subject = claimsIdentity,
                                        SigningCredentials = signingCredentials,
                                    };
                if (notBefore != null)
                {
                    securityTokenDescriptor
                        .NotBefore = notBefore;
                }
                if (expires != null)
                {
                    securityTokenDescriptor
                        .Expires = expires;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
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