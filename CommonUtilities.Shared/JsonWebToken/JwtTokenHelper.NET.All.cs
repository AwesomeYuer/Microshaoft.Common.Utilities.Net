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

        //private static string _webTokenPlainTextSecretKey
        //        = //"ABCDEFGHABCDEFGHABCDEFGHABCDEFGHABCDEFGH";
        //            ConfigurationManager
        //                .AppSettings["WebTokenSecretKey"];
        //private static string[] _webTokenIssuers = new string[] { "SOPS" };
        //private static string[] _webTokenAudiences = new string[] { "SOPS" };

        //private static int _webTokenExpireInSeconds
        //        = int.Parse
        //                (
        //                    "10000"
        //                    //ConfigurationManager
        //                    //    .AppSettings["WebTokenExpiredInSeconds"]
        //                );


        //private static string _ssoTokenPlainTextSecretKey 
        //        = //"ZBCDEFGHZBCDEFGHZBCDEFGHZBCDEFGHZBCDEFGH";
        //            ConfigurationManager
        //                .AppSettings["SsoTokenSecretKey"];
        //private static string[] _ssoTokenIssuers = new string[] { "BL" };
        //private static string[] _ssoTokenAudiences = new string[] { "SOPS" };
        //private static int _ssoTokenExpireInSeconds
        //        = int.Parse
        //                (
        //                    "10000"
        //                    //ConfigurationManager
        //                    //    .AppSettings["SsoTokenExpiredInSeconds"]
        //                );

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
                            , claims
                            , out plainToken
                            , out secretTokenString
                            , signingKey
                            , signingCredentials
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
                        , IEnumerable<Claim> claims
                        , out Microsoft.IdentityModel.Tokens.SecurityToken plainToken
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
                    )
        {
            bool r = false;
            plainToken = null;
            secretTokenString = null;
            try
            {
                //var signingKey = new Microsoft
                //                           .IdentityModel
                //                           .Tokens
                //                           .SymmetricSecurityKey
                //                                   (
                //                                       Encoding
                //                                           .GetEncoding(plainTextSecurityKeyEncoding)
                //                                           .GetBytes(plainTextSecurityKey)
                //                                   );
                //var signingCredentials
                //            = new Microsoft.IdentityModel.Tokens.SigningCredentials
                //                (
                //                    signingKey
                //                    , signingCredentialsAlgorithm
                //                    , signingCredentialsDigest
                //                );
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
            catch (Exception e)
            {

                //throw;
            }
            return
                   r;
        }

        

    }
}
#endif