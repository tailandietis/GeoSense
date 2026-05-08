using System;

namespace GeoDa.Application.AlarmMonitors.Repository.AlarmItems.Dtos;

public class AlarmItemDto
{
    public long Id { get; set; }
    public int ModuleFamilyCode { get; set; }
    public int ObjectId { get; set; }
    public int ServiceFamilyCode { get; set; }
    public int ErrorCode { get; set; }
    public int Status { get; set; }
    public DateTime Dt { get; set; }
}
