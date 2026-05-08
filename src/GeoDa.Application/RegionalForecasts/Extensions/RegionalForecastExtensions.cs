using GeoDa.Application.Exceptions;
using GeoDa.Application.RegionalForecasts.Repository.Events.Dtos;
using GeoDa.Application.RegionalForecasts.Repository.GsAlarms.Dtos;
using GeoDa.Application.RegionalForecasts.Repository.GsStats.Dtos;
using GeoDa.Domain.Models;
using GeoDa.Domain.RegionalForecasts.Models;
using System;

namespace GeoDa.Application.RegionalForecasts.Extensions;

internal static class RegionalForecastExtensions
{
    public static GsAlarmDto ToGsAlarmDto(this EnergyAssessment energy)
    {
        var result = new GsAlarmDto()
        {
            Obj = energy.Object.ObjectId,
            Dt = energy.Object.DateTimeOfAssessment,
            Quality = (int)energy.Report.Quality,
            AlarmStatus = energy.Report.IsAlarm,
            ItemDt = energy.Report.DateTimeOfItem,
            E = (float)energy.Report.Energy,
            ELim = (float)energy.Object.EnergyLimit,
            CheckInterval = energy.Object.CheckIntervalInDays
        };

        return result;
    }

    public static GsStatDto ToGsStatDto(this ObjectStatistics objectStat)
    {
        var result = new GsStatDto()
        {
            Obj = objectStat.ObjectId,
            Dt = objectStat.DateTimeOfStatCalc,
            Quality = (int)objectStat.Quality,
            MinVal = objectStat.Statistics.MinVal,
            MaxVal = objectStat.Statistics.MaxVal,
            Q70 = objectStat.Statistics.Q70,
            Q80 = objectStat.Statistics.Q80,
            Q90 = objectStat.Statistics.Q90,
            Q95 = objectStat.Statistics.Q95,
            Q99 = objectStat.Statistics.Q99,
            NRows = objectStat.Statistics.NRows,
            DtOldest = objectStat.Statistics.DateTimeOfOldestEvent,
            DtNewest = objectStat.Statistics.DateTimeOfNewestEvent,
            StatCalcInterval = objectStat.StatCalcInterval
        };

        return result;
    }

    public static Event ToEvent(this EventDto eventDto, Func<int, int, DateTime> dtConvertor)
        => MapToEvent(eventDto, dtConvertor);

    public static bool IsSomeProperitiesIsNull(this EventDto eventFromDb)
    {
        if (eventFromDb.X is null
            || eventFromDb.Y is null
            || eventFromDb.Z is null
            || eventFromDb.E is null
            || eventFromDb.Ampl is null
            || eventFromDb.Magn is null
            || eventFromDb.Proc is null)
            return true;
        else
            return false;
    }

    // Private
    private static Event MapToEvent(EventDto eventDto, Func<int, int, DateTime> dtConvertor)
    {
        if (eventDto == null)
            throw new ArgumentNullException(nameof(eventDto));

        if (eventDto.X is null
            || eventDto.Y is null
            || eventDto.Z is null
            || eventDto.E is null
            || eventDto.Ampl is null
            || eventDto.Magn is null
            || eventDto.Proc is null)
            throw new GeoDaAppException(OpStatus.CastModelError, "eventDto: One of event's field is null");

        try
        {
            return new Event
            {
                Id = eventDto.Obj,
                Dt = dtConvertor(eventDto.Idat, eventDto.Itim),
                N = eventDto.N,
                X = (int)eventDto.X,
                Y = (int)eventDto.Y,
                Z = (int)eventDto.Z,
                E = (float)eventDto.E,
                Magn = (float)eventDto.Magn,
                Proc = (float)eventDto.Proc,
                Ampl = (float)eventDto.Ampl,
                NpActual = eventDto.NpActual,
                RqMin = eventDto.RqMin,
                RqMax = eventDto.RqMax,
                GpActual = eventDto.GpActual,
                AmplMax = eventDto.AmplMax,
                EMax = eventDto.EMax
            };
        }
        catch (Exception ex)
        {
            throw new GeoDaAppException(OpStatus.CastModelError, ex.Message);
        }
    }
}