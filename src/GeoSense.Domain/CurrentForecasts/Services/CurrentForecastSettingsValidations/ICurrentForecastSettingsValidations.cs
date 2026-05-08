using GeoDa.Domain.CurrentForecasts.Models.Settings;

namespace GeoDa.Domain.CurrentForecasts.Services.CurrentForecastSettingsValidations;

public interface ICurrentForecastSettingsValidations
{
    bool IsSettingsValid(CurrentForecastSettings cfSettings);
}
