namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Security.Claims;
    public static class ClaimsHelper
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
                                                        x
                                                            .GetValue(null)
                                                            .ToString();
                                                }
                                                , StringComparer.OrdinalIgnoreCase
                                            );
                            }
                        )();

        public static IEnumerable<Claim> AsClaims(this JToken target)
        {
               var jValues = target
                                .GetAllJValues();
               return
                    jValues
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

    }


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
                r = true;
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
        public static string GetClaimTypeValueOrDefault
                                (
                                    this ClaimsPrincipal target
                                    , string claimType
                                    , string defaultValue = null
                                )

        {
            var r = defaultValue;
            if
                (
                    TryGetClaimTypeValue
                        (
                            target
                            , claimType
                            , out var @value
                        )
                )
            {
                r = @value;
            }
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
                                )
                            .Value;
                r = true;
            }
            return r;
        }
        public static DateTime? GetIssuedAtLocalTime(this ClaimsPrincipal target)
        {
            DateTime? r = null;
            var b = target
                        .TryGetClaimTypeValue
                            (
                                "iat"
                                , out var claimValue
                            );
            if (b)
            {
                b = double
                        .TryParse
                            (
                                claimValue
                                , out var seconds
                            );
                if (b)
                {
                    r = ParseSecondsToLocalTime
                            (
                                seconds
                            );
                }
            }
            return r;
        }


        public static IPAddress GetClientIP
                                    (
                                        this ClaimsPrincipal target
                                        , string claimType = "ip"
                                    )
        {
            IPAddress r = null;
            if 
                (
                    target
                        .TryGetClaimTypeValue
                            (
                                claimType
                                , out var claimValue
                            )
                )
            {
                _ = IPAddress
                        .TryParse
                            (
                                claimValue
                                , out r
                            );
            }
            return r;
        }

        public static DateTime ParseSecondsToLocalTime(double seconds)
        {
            return new DateTime(1970, 1, 1).AddSeconds(seconds).ToLocalTime();
        }
    }
}
