#if NETFRAMEWORK4_X
namespace Microshaoft.WebApi.Versioning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web.Http.Routing;

    internal class VersionConstraint : IHttpRouteConstraint
    {
        public const string VersionQueryString = "v";

        public const string VersionHeaderName = "api-version";

        public const string AcceptHeaderName = "Accept";
        public const string AcceptVersionRegex = @"application\/vnd\.gnap\.v([\d]+)\+json";

        private const int DefaultVersion = 1;

        public int AllowedVersion { get; private set; }

        public VersionConstraint(int allowedVersion)
        {
            AllowedVersion = allowedVersion;
        }

        public bool Match
                    (
                        HttpRequestMessage request
                        , IHttpRoute route
                        , string parameterName
                        , IDictionary<string, object> values
                        , HttpRouteDirection routeDirection
                    )
        {
            if (routeDirection != HttpRouteDirection.UriResolution)
            {
                return true;
            }
            var version = GetVersionHeader(request) ?? DefaultVersion;

            return (version == AllowedVersion);
        }

        private int? GetVersionHeader(HttpRequestMessage request)
        {
            var versionAsString = GetQueryString(request, VersionQueryString) ??
                                  GetHeaderValue(request, VersionHeaderName) ??
                                  GetRegexVersion
                                        (
                                            AcceptVersionRegex
                                            , GetHeaderValue(request, AcceptHeaderName)
                                        );

            int version;
            if (versionAsString != null && Int32.TryParse(versionAsString, out version))
                return version;

            return null;
        }

        private string GetQueryString(HttpRequestMessage request, string queryName)
        {
            var queryString = request.RequestUri.Query;

            if (string.IsNullOrWhiteSpace(queryString))
            {
                return null;
            }
            var queryParts = request.RequestUri.ParseQueryString();
            return queryParts.Get(queryName);
        }

        private string GetHeaderValue(HttpRequestMessage request, string headerName)
        {
            IEnumerable<string> headerValues;

            if (request.Headers.TryGetValues(headerName, out headerValues) && headerValues.Count() == 1)
                return headerValues.First();

            return null;
        }

        private string GetRegexVersion(string pattern, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var regex = Regex.Match(value, pattern, RegexOptions.IgnoreCase);

            if (!regex.Success)
                return null;

            if (regex.Groups.Count == 2)
                return regex.Groups[1].Value;

            return null;
        }
    }
}
#endif
