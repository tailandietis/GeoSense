using GeoDa.Domain.RegionalForecasts.Models;
using System.Collections.Generic;

namespace GeoDa.Application.RegionalForecasts.Services.VolumeMaps;

public interface IRfVolumeMapsService
{
    /// <summary>
    /// Генерирует 3D HTML-карту событий через volumetricbuilder и копирует результат в wwwroot.
    /// </summary>
    /// <param name="objectId">Идентификатор объекта.</param>
    /// <param name="events">Список сейсмических событий.</param>
    /// <param name="layouts">Подложки 3D-карты (1–2 штуки). Если не указаны или пусты — карта строится без фоновых изображений.</param>
    /// <returns>Относительный URL вида /img/events_volume_1_2026-03-17-10-30-00.html, либо null при ошибке.</returns>
    string? CreateVolumeMap(int objectId, IReadOnlyList<Event> events, IReadOnlyList<VolumeLayoutConfig>? layouts = null);
}
