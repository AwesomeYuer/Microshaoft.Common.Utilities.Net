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

        public static IEnumerable<Claim> AsClaims(this JToken @this)
        {
               var jValues = @this
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
                                    this ClaimsPrincipal @this
                                    , string claimType
                                    , out JToken claimValue
                                )
        {
            claimValue = null;
            var r = @this
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
                                    this ClaimsPrincipal @this
                                    , string claimType
                                )
        {
            var json = GetClaimTypeValue(@this, claimType);
            return
                JToken.Parse(json);
        }
        public static string GetClaimTypeValue
                                (
                                    this ClaimsPrincipal @this
                                    , string claimType
                                )
        {
            var r = @this
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
                                    this ClaimsPrincipal @this
                                    , string claimType
                                    , string defaultValue = null
                                )

        {
            var r = defaultValue;
            if
                (
                    TryGetClaimTypeValue
                        (
                            @this
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
                                    this ClaimsPrincipal @this
                                    , string claimType
                                    , out string claimValue
                                )
        {
            claimValue = string.Empty;
            var r = @this
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
                        @this
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
        public static DateTime? GetIssuedAtLocalTime(this ClaimsPrincipal @this)
        {
            DateTime? r = null;
            var b = @this
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
                                        this ClaimsPrincipal @this
                                        , string claimType = "ip"
                                    )
        {
            IPAddress r = null;
            if 
                (
                    @this
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
