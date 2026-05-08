using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoDa.Domain.GeneralForecasts.Models;

public record MsgLog(int Id, DateTime DateTime, int N, int MsgCode, int ErrorCode, string Information);
