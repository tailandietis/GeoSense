using System.IO;
using System.Text.Json;

namespace GeoDa.Infrastructure.Services.JsonFiles;

internal class JsonFileService : IJsonFileService
{
    public T Load<T>(string fileName)
    {
        if (!File.Exists(fileName))
            throw new FileNotFoundException($"File: {fileName}");

        T? config = JsonSerializer.Deserialize<T>(File.ReadAllText(fileName));

        if (config is null)
            throw new FileLoadException(fileName);
        else
            return config;
    }

    public void Save<T>(string fileName, T config)
    {
        using var sw = new StreamWriter(fileName);

        sw.Write(JsonSerializer.Serialize(config,
            new JsonSerializerOptions { WriteIndented = true }));
    }
}
