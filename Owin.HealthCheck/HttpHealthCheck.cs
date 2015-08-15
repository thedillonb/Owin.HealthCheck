using System;
using System.Net;
using System.Threading.Tasks;

namespace Owin.HealthCheck
{
    public class HttpHealthCheck : BaseHealthCheck
    {
        private readonly Uri _uri;
        private readonly ICredentials _credentials;

        public HttpHealthCheck(string name, Uri uri, ICredentials credentials = null)
            : base(name)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            _uri = uri;
            _credentials = credentials;
        }

        public override async Task<HealthCheckStatus> Check()
        {
            var request = (HttpWebRequest)WebRequest.Create(_uri);
            request.UserAgent = "Owin.HealthCheck";
            request.Method = "GET";
            request.Credentials = _credentials;

            using (var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
            {
                var statusCode = (int)response.StatusCode;
                if (statusCode >= 200 && statusCode < 300)
                    return HealthCheckStatus.Passed(response.StatusDescription);
                else
                    return HealthCheckStatus.Failed(response.StatusDescription);
            }
        }
    }
}
