using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using YnabApi.Files;

namespace YnabApi.Dropbox
{
    public class DropboxFileSystem : IFileSystem
    {
        private readonly HttpClient _client;

        public DropboxFileSystem(string accessToken)
        {
            this._client = new HttpClient(new CachingMessageHandler(new AccessTokenMessageHandler(accessToken, new HttpClientHandler())));
        }

        public async Task<string> ReadFileAsync(string file)
        {
            HttpResponseMessage result = await this._client
                .GetAsync($"https://content.dropboxapi.com/1/files/auto/{file}");
            
            if (result.StatusCode != HttpStatusCode.OK)
                return string.Empty;
            
            return await result.Content.ReadAsStringAsync();
        }

        public async Task<IList<string>> GetFilesAsync(string directory)
        {
            HttpResponseMessage result = await this._client
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
            return this._client
                .PutAsync($"https://content.dropboxapi.com/1/files_put/auto/{file}", new StringContent(content));
        }

        public Task CreateDirectoryAsync(string directory)
        {
            return Task.FromResult((object)null);
        }
    }
}
