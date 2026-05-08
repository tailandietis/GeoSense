namespace GeoDa.Domain.AlarmMonitors.Models.Factories;

internal static class AlarmCodeModelFactory
{
    public static AlarmCode Create(int moduleFamilyCode, int serviceFamilyCode, int errorCode) =>
        new(ModuleFamilyCode: moduleFamilyCode, ServiceFamilyCode: serviceFamilyCode, ErrorCode: errorCode);
}
