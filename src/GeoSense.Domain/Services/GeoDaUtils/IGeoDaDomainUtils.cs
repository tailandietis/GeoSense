using GeoDa.Domain.Models;
using System;

namespace GeoDa.Domain.Services.GeoDaUtils;

public interface IGeoDaDomainUtils
{
    int GetHourOffset();

    (DateTime startDay, DateTime endDay) CreateDateInterval(int intervalInDays, DateTime referenceDateTime);

    DateTime ConvertAzonFormatToDateTime(int date, int time);

    DateTime ConvertAzonFormatToDateTime(int date);

    int GetTimeInAzonFormat(DateTime dt);

    int GetDateInAzonFormat(DateTime dt);

    string GetStringHash(string str);

    ObjectStatus BuildObjectStatusByOpStatus(OpStatus opStatus);
}
