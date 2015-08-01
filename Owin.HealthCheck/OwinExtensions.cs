using Microsoft.Owin;
using System;
using System.Collections.Generic;

namespace Owin.HealthCheck
{
    static class OwinRequestExtensions
    {
        public static Dictionary<string, string> GetQueryParameters(this IOwinRequest request)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var pair in request.Query)
            {
                var value = string.Join(",", pair.Value);
                dictionary.Add(pair.Key, value);
            }

            return dictionary;
        }
    }
}
