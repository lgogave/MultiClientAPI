using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Routing;

namespace ExpenseTracker.API.Attributes
{
    public class VersionConstraint : IHttpRouteConstraint
    {
        public const string VersionHeaderName = "api-version";
        private const int DefaultVersion = 1;

        public VersionConstraint(int allowedVersion)
        {
            AllowedVersion = allowedVersion;
        }

        public int AllowedVersion
        {
            get;    
            private set;
        }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (routeDirection == HttpRouteDirection.UriResolution)
            {
                int? version = GetVersionFromCustomHeader(request);
                return ((version ?? DefaultVersion) == AllowedVersion);
            }
            return true;
        }

        private int? GetVersionFromCustomHeader(HttpRequestMessage request)
        {
            string versionAsString;
            IEnumerable<string> headersValues;
            if (request.Headers.TryGetValues(VersionHeaderName, out headersValues) && headersValues.Count() == 1)
            {
                versionAsString = headersValues.First();
            }
            else
            {
                return null;
            }
            int version;
            if (versionAsString != null && Int32.TryParse(versionAsString, out version))
            {
                return version;
            }
            return null;
        }


    }
}