using GeoDa.Domain.Models;
using GeoDa.Domain.SupportPosts.Models;
using System;
using System.Collections.Generic;

namespace GeoDa.Domain.SupportPosts.Services;

public interface ISupportPostDataFactory
{
    SupportPostData CreateDefault();

    SupportPostData Create(
        int objectId,
        string objectName,
        Quality objectQuality,
        DateTime dateTime,
        List<SupportPostParameter> parameters);
}
