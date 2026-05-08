using GeoDa.Domain.Models;
using GeoDa.Domain.SupportPosts.Models;

namespace GeoDa.Domain.SupportPosts.Services;

public interface ISupportPostParameterFactory
{
    SupportPostParameter CreateDefault();

    SupportPostParameter Create(int id, string name, string unit, float value, Quality quality);
}
