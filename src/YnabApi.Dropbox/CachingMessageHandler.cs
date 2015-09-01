using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace YnabApi.Dropbox
{
    internal class CachingMessageHandler : DelegatingHandler
    {
        private readonly ConcurrentDictionary<Tuple<HttpMethod, Uri>, Tuple<string, byte[]>>  _cache;

        public CachingMessageHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this._cache = new ConcurrentDictionary<Tuple<HttpMethod, Uri>, Tuple<string, byte[]>>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Tuple<string, byte[]> cached;
            if (this._cache.TryGetValue(Tuple.Create(request.Method, request.RequestUri), out cached))
            {
                request.Headers.TryAddWithoutValidation("If-None-Match", cached.Item1);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotModified && cached != null)
            {
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new ByteArrayContent(cached.Item2);

                return response;
            }

            IEnumerable<string> allEtags;
            if (response.Headers.TryGetValues("Etag", out allEtags))
            {
                string etag = allEtags.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(etag) == false)
                { 
                    this._cache[Tuple.Create(request.Method, request.RequestUri)] = Tuple.Create(etag, await response.Content.ReadAsByteArrayAsync());
                }
            }

            return response;
        }
    }
}