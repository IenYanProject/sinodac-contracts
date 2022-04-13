using System.Collections.Generic;

namespace Sinodac.Contracts.Delegator.Strategies
{
    public interface IPermissionIgnoreStrategy
    {
        List<string> GetIgnoredPermissionList();
    }
}