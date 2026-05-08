using System.Collections.Generic;

namespace GeoDa.Application.Authentication.StateProvider;

public static class Role
{
    public static readonly IList<string> Names = new List<string>()
    {
        "administrator",
        "dispatcher"
    }.AsReadOnly();
}
