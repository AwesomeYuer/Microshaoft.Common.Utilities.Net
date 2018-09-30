namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;
    using System.Security.Claims;
    public static class CliamsHelper
    {
        public static bool TryGetClaimTypeJTokenValue
                                (
                                    this ClaimsPrincipal target
                                    , string claimType
                                    , out JToken claimValue
                                )
        {
            claimValue = null;
            var r = target
                        .TryGetClaimTypeValue
                            (
                                claimType
                                , out var s
                            );
            if (r)
            {
                claimValue = JToken.Parse(s);
            }
            return r;
        }
        public static JToken GetClaimTypeJTokenValue
                                (
                                    this ClaimsPrincipal target
                                    , string claimType
                                )
        {
            var json = GetClaimTypeValue(target, claimType);
            return
                JToken.Parse(json);
        }
        public static string GetClaimTypeValue
                                (
                                    this ClaimsPrincipal target
                                    , string claimType
                                )
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

        public static bool TryGetClaimTypeValue
                                (
                                    this ClaimsPrincipal target
                                    , string claimType
                                    , out string claimValue
                                )
        {
            claimValue = string.Empty;
            var r = target
                        .HasClaim
                            (
                                (x) =>
                                {
                                    return
                                        (
                                            string
                                                .Compare
                                                    (
                                                        x.Type
                                                        , claimType
                                                        , true
                                                    )
                                            == 0
                                        );
                                }
                            );
            if (r)
            {
                claimValue =
                        target
                            .Claims
                            .FirstOrDefault
                                (
                                    (x) =>
                                    {
                                        return
                                            (
                                                x.Type
                                                ==
                                                claimType
                                            );
                                    }
                                ).Value;
            }
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
        //public static bool TryValidateSsoToken
        //                (
        //                    string token
        //                    , out ClaimsPrincipal claimsPrincipal
        //                    , string ip = null
        //                //, int expiredInSeconds = -1
        //                )
        //{
        //    var r = false;
        //    var plainTextSecurityKey = _ssoTokenPlainTextSecretKey;
        //    var issuers = _ssoTokenIssuers;
        //    var audiences = _ssoTokenAudiences;
        //    SecurityToken validatedToken = null;
        //    r = TryValidateToken
        //                (
        //                    plainTextSecurityKey
        //                    , token
        //                    , issuers
        //                    , audiences
        //                    , out validatedToken
        //                    , out claimsPrincipal
        //                );
        //    if (r)
        //    {
        //        IPAddress ipa;
        //        if (IPAddress.TryParse(ip, out ipa))
        //        {
        //            r = (claimsPrincipal.GetClaimTypeValue("UserClientIP") == ip);
        //        }
        //    }
        //    if (r)
        //    {
        //        if (_ssoTokenExpireInSeconds >= 0)
        //        {
        //            var issuedAt = claimsPrincipal.GetIssuedAtTime();
        //            r = (Math.Abs(DateTimeHelper.SecondsDiffNow(issuedAt)) < _ssoTokenExpireInSeconds);
        //        }
        //    }
        //    if (!r)
        //    {
        //        claimsPrincipal = null;
        //    }
        //    return r;
        //}
        public static DateTime ParseSecondsToLocalTime(double seconds)
        {
            return new DateTime(1970, 1, 1).AddSeconds(seconds).ToLocalTime();
        }
    }
}
