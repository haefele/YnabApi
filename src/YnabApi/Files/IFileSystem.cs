using System.Collections.Generic;
using System.Threading.Tasks;

namespace YnabApi.Files
{
    public interface IFileSystem
    {
        Task<string> ReadFileAsync(string file);
        Task<IList<string>> GetFilesAsync(string directory);
        Task WriteFileAsync(string file, string content);
        Task CreateDirectoryAsync(string directory);
        Task FlushWritesAsync();
    }
}