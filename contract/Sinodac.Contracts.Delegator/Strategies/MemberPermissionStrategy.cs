using System.Collections.Generic;
using Sinodac.Contracts.Delegator.Helpers;

namespace Sinodac.Contracts.Delegator.Strategies
{
    public class MemberPermissionStrategy : IPermissionIgnoreStrategy
    {
        public List<string> GetIgnoredPermissionList()
        {
            return PermissionHelper.GetUserRelatedPermissionIdList();
        }
    }
}