using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoDa.Application.GeneralForecasts.Repository.MsgCodeTexts.Dtos;

public class ErrorCodeTextDto
{
    public int Code { get; set; }
    public string Text { get; set; } = string.Empty;
}
