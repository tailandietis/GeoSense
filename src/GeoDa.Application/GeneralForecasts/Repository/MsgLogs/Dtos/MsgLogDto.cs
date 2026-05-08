using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoDa.Application.GeneralForecasts.Repository.MsgLogs.Dtos;

public class MsgLogDto
{
    public int ObjectId { get; set; }
    public int Date { get; set; }
    public int Time { get; set; }
    public int N { get; set; }
    public int MsgCode { get; set; }
    public int ErrorCode { get; set; }
    public string Information { get; set; } = string.Empty;
}
