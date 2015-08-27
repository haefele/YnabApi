using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using YnabApi.Files;

namespace YnabApi.Dropbox
{
    public class DropboxFileSystem : IFileSystem
    {
        private readonly string _accessToken;

        public DropboxFileSystem(string accessToken)
        {
            this._accessToken = accessToken;
        }

        public async Task<string> ReadFileAsync(string file)
        {
            HttpResponseMessage result = await this
                .GetHttpClient()
                .GetAsync($"https://content.dropboxapi.com/1/files/auto/{file}");

            if (result.StatusCode != HttpStatusCode.OK)
                return string.Empty;

            return await result.Content.ReadAsStringAsync();
        }

        public async Task<IList<string>> GetFilesAsync(string directory)
        {
            HttpResponseMessage result = await this
                .GetHttpClient()
                .GetAsync($"https://api.dropboxapi.com/1/metadata/auto/{directory}");

            if (result.StatusCode != HttpStatusCode.OK)
                return new List<string>();

            string resultAsString = await result.Content.ReadAsStringAsync();
            var json = JObject.Parse(resultAsString);

            return json
                .Value<JArray>("contents")
                .Values<JObject>()
                .Select(f => new {IsDirectory = f.Value<bool>("is_dir"), Path = f.Value<string>("path")})
                .Where(f => f.IsDirectory == false)
                .Select(f => f.Path)
                .ToList();
        }

        public Task WriteFileAsync(string file, string content)
        {
            return this
                .GetHttpClient()
                .PutAsync($"https://content.dropboxapi.com/1/files_put/auto/{file}", new StringContent(content));
        }

        public Task CreateDirectoryAsync(string directory)
        {
            return Task.FromResult((object)null);
        }

        private HttpClient GetHttpClient()
        {
            var client = new HttpClient(new AccessTokenMessageHandler(this._accessToken, new HttpClientHandler()));
            return client;
        }
    }

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
