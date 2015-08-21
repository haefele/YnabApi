using System.IO;
using System.Threading.Tasks;

namespace Ynab.Files
{
    public static class FileHelpers
    {
        public static Task<string> ReadFileAsync(string file)
        {
            return ReadFileAsync(new FileInfo(file));
        }

        public static async Task<string> ReadFileAsync(FileInfo file)
        {
            using (var fileStream = file.OpenRead())
            using (var reader = new StreamReader(fileStream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}