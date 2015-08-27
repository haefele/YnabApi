using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using YnabApi.Files;

namespace YnabApi.Universal
{
    public class UniversalFileSystem : IFileSystem
    {
        private readonly StorageFolder _rootFolder;

        public UniversalFileSystem(StorageFolder rootFolder)
        {
            this._rootFolder = rootFolder;
        }

        public async Task<string> ReadFileAsync(string file)
        {
            using (var stream = await this._rootFolder.OpenStreamForReadAsync(file))
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public async Task<IList<string>> GetFilesAsync(string directory)
        {
            var directoryInfo = await this._rootFolder.GetFolderAsync(directory);
            var allFiles = await directoryInfo.GetFilesAsync();

            return allFiles
                .Select(f => f.Name)
                .Select(f => Path.Combine(directory, f))
                .ToList();
        }

        public async Task WriteFileAsync(string file, string content)
        {
            using (var stream = await this._rootFolder.OpenStreamForWriteAsync(file, CreationCollisionOption.ReplaceExisting))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(content);
            }
        }

        public async Task CreateDirectoryAsync(string directory)
        {
            await this._rootFolder.CreateFolderAsync(directory);
        }
    }
}