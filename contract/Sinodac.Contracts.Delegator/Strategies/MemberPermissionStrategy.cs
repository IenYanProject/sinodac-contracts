using System;
using System.Collections.Generic;
using System.Linq;
using Sinodac.Contracts.Delegator.Helpers;

namespace Sinodac.Contracts.Delegator.Strategies
{
    public class MemberPermissionStrategy : IPermissionSetStrategy
    {
        public List<string> ExtractPermissionList(List<string> rolePermissionList)
        {
            var permissionList = rolePermissionList;
            foreach (var permissionId in PermissionHelper.GetUserRelatedPermissionIdList()
                         .Where(permissionId => permissionList.Contains(permissionId)))
            {
                permissionList.Remove(permissionId);
            }

            return permissionList;
        }
    }
}