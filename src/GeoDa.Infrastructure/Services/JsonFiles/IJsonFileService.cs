namespace GeoDa.Infrastructure.Services.JsonFiles;

public interface IJsonFileService
{
    T Load<T>(string fileName);

    void Save<T>(string fileName, T config);
}
