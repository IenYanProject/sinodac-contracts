using System.Collections.Generic;

namespace Sinodac.Contracts.Delegator.Strategies
{
    public class AdminPermissionIgnoreStrategy : IPermissionIgnoreStrategy
    {
        public List<string> GetIgnoredPermissionList()
        {
            return new List<string>();
        }
    }
}