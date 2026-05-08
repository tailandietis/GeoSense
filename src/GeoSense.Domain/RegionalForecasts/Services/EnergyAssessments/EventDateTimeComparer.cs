using GeoDa.Domain.RegionalForecasts.Models;
using System.Collections.Generic;

namespace GeoDa.Domain.RegionalForecasts.Services.EnergyAssessments;

public class EventDateTimeComparer : IComparer<Event>
{
    public int Compare(Event? x, Event? y)
    {
        if (x is null && y is null)
            return 0;
        else if (x is null)
            return -1;
        else if (y is null)
            return 1;
        else
        {
            if (x.Dt > y.Dt)
                return 1;
            else if (x.Dt < y.Dt)
                return -1;
            else
                return 0;
            //if (x.Idat > y.Idat)
            //    return 1;
            //else if (x.Idat < y.Idat)
            //    return -1;
            //else
            //{
            //    if (x.Itim > y.Itim)
            //        return 1;
            //    else if (x.Itim < y.Itim)
            //        return -1;
            //    else
            //        return 0;
            //}
        }
    }
}
