using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace evidence_timeline.Utilities
{
    public static class JsonHelper
    {
        public static JsonSerializerOptions DefaultOptions { get; } = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public static async Task<T?> LoadAsync<T>(string filePath)
        {
            await using var stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<T>(stream, DefaultOptions);
        }

        public static async Task SaveAsync<T>(string filePath, T obj)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, obj, DefaultOptions);
        }
    }
}
