namespace GeoDa.BlazorWebApp.Services.MLBuilder;

public class MLBuilderSettings
{
    public string BuilderDir { get; set; } = "./mlbuilder";
    public string OutputDir { get; set; } = "./mlbuilder/output";
    public string ThesisFiguresDir { get; set; } = "";
    public string RealEventsPath { get; set; } = "";
    public string GeoDaDataDir { get; set; } = "";
}
