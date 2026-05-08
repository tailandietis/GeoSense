namespace GeoDa.Application.RegionalForecasts.Repository.Events.Dtos;

public class EventDto
{
    public int Obj { get; set; }
    public int Idat { get; set; }
    public int Itim { get; set; }
    public int N { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public int? Z { get; set; }
    public float? E { get; set; }
    public float? Magn { get; set; }
    public float? Proc { get; set; }
    public float? Ampl { get; set; }
    public int? NpActual { get; set; }
    public float? RqMin { get; set; }
    public float? RqMax { get; set; }
    public string? GpActual { get; set; }
    public float? AmplMax { get; set; }
    public float? EMax { get; set; }
}
