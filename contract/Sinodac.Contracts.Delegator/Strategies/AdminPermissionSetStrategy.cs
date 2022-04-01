using System.Collections.Generic;

namespace Sinodac.Contracts.Delegator.Strategies
{
    public class AdminPermissionSetStrategy : IPermissionSetStrategy
    {
        public List<string> ExtractPermissionList(List<string> rolePermissionList)
        {
            return rolePermissionList;
        }
    }
}