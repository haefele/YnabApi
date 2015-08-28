using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace YnabApi.Dropbox
{
    internal class AccessTokenMessageHandler : DelegatingHandler
    {
        private readonly string _accessToken;

        public AccessTokenMessageHandler(string accessToken, HttpMessageHandler innerHandler) : base(innerHandler)
        {
            this._accessToken = accessToken;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this._accessToken);

            return base.SendAsync(request, cancellationToken);
        }
    }
}