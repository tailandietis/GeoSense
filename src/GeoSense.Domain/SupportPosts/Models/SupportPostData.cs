using GeoDa.Domain.Models;
using System;
using System.Collections.Generic;

namespace GeoDa.Domain.SupportPosts.Models;

public record SupportPostData(
    int ObjectId,
    string ObjectName,
    Quality ObjectDataQuality,
    DateTime DateTime,
    List<SupportPostParameter> Parameters);