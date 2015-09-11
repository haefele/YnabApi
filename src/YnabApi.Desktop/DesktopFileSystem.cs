using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YnabApi.Files;

namespace YnabApi.Desktop
{
    public class DesktopFileSystem : IFileSystem
    {
        public async Task<string> ReadFileAsync(string file)
        {
            using (var fileStream = File.Open(this.ToFullPath(file), FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(fileStream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public Task<IList<string>> GetFilesAsync(string directory)
        {
            var directoryInfo = new DirectoryInfo(this.ToFullPath(directory));

            if (directoryInfo.Exists == false)
                return Task.FromResult<IList<string>>(new List<string>());

            IList<string> files = directoryInfo
                .GetFiles()
                .Select(f => f.FullName)
                .Select(f => f.Substring(this.GetRootDirectory().Length))
                .Select(f => f.TrimStart('\\', '/'))
                .ToList();

            return Task.FromResult(files);
        }

        public async Task WriteFileAsync(string file, string content)
        {
            using (var fileStream = File.Open(this.ToFullPath(file), FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(fileStream))
            {
                await writer.WriteAsync(content);
            }
        }

        public Task CreateDirectoryAsync(string directory)
        {
            Directory.CreateDirectory(this.ToFullPath(directory));

            return Task.CompletedTask;
        }

        public Task FlushWritesAsync()
        {
            return Task.CompletedTask;
        }

        private string GetRootDirectory()
        {
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string dropboxFolderName = "Dropbox";

            return Path.Combine(userFolder, dropboxFolderName);
        }

        private string ToFullPath(string path)
        {
            var rootDirectory = this.GetRootDirectory();
            return Path.Combine(rootDirectory, path);
        }
    }
}