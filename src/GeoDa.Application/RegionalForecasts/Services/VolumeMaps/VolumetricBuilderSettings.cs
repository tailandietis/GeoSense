namespace GeoDa.Application.RegionalForecasts.Services.VolumeMaps;

public class VolumetricBuilderSettings
{
    /// <summary>
    /// Путь к директории volumetricbuilder (содержит exe, data/, settings/, wwwroot/img/).
    /// Может быть абсолютным или относительным к рабочей директории приложения.
    /// </summary>
    public string BuilderDir { get; set; } = string.Empty;

    /// <summary>
    /// Путь к wwwroot/img/ Blazor-приложения, куда копируется готовый HTML для отдачи браузеру.
    /// Может быть абсолютным или относительным к рабочей директории приложения.
    /// </summary>
    public string WebRootImgPath { get; set; } = string.Empty;

    /// <summary>
    /// Путь к директории mlbuilder (содержит generate_3d_map.py и venv/).
    /// Если указан — используется улучшенная Python-визуализация вместо volume_builder.exe.
    /// </summary>
    public string MlBuilderDir { get; set; } = string.Empty;
}
