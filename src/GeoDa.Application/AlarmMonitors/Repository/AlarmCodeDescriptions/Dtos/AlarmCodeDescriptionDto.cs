namespace GeoDa.Application.AlarmMonitors.Repository.AlarmCodeDescriptions.Dtos;

public class AlarmCodeDescriptionDto
{
    public int ModuleFamilyCode { get; set; }
    public int ServiceFamilyCode { get; set; }
    public int ErrorCode { get; set; }
    public string Msg { get; set; } = string.Empty;
}
