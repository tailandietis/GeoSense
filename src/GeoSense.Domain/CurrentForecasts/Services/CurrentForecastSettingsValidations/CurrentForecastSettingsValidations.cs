using GeoDa.Domain.CurrentForecasts.Models.Settings;
using System;

namespace GeoDa.Domain.CurrentForecasts.Services.CurrentForecastSettingsValidations;

internal class CurrentForecastSettingsValidations : ICurrentForecastSettingsValidations
{
    // ICurrentForecastSettingsValidations
    public bool IsSettingsValid(CurrentForecastSettings cfSettings)
    {
        /* TODO: Написать процедуру валидации настроек
         * 
         * # Требования
         * 
         * ## CurrentForecastGeneralSettings
         * 
         * FeaturesUpdatePeriodInSeconds >= 1
         * 
         * ## CurrentForecastObjectSettings
         * 
         * DSliceLL > 0
         * DSliceHH > 0
         * DBlockLL > 0
         * DSliceHH > DSliceLL
         * BlockX0 < BlockX1
         * BlockY0 < BlockY1
         * MaxDValue > 0
        */
        throw new NotImplementedException();
    }
}
