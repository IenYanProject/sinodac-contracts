using System.Collections.Generic;
using AElf.Sdk.CSharp;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        private void CheckPermission(string fromId, string actionId)
        {
            var user = State.UserMap[fromId];
            if (user == null)
            {
                throw new AssertionException($"用户 {fromId} 不存在");
            }

            Assert(user.Enabled, $"用户 {fromId} 当前为禁用状态");

            if (user.OrganizationName == null)
            {
                // User isn't belongs to any organization unit.
                throw new AssertionException($"用户 {fromId} 不属于任何机构");
            }

            var organizationUnit = State.OrganizationUnitMap[user.OrganizationName];
            if (organizationUnit == null)
            {
                throw new AssertionException($"机构 {user.OrganizationName} 不存在");
            }

            Assert(organizationUnit.Enabled, $"机构 {organizationUnit.OrganizationName} 当前为禁用状态");
            Assert(State.RolePermissionMap[organizationUnit.RoleName][actionId],
                $"角色{organizationUnit.RoleName}没有调用 {actionId} 的权限");
        }

        private List<string> GetAllActionIds()
        {
            return new List<string>
            {
                Permissions.Roles.Default,
                Permissions.Roles.Create,
                Permissions.Roles.Update,
                Permissions.Roles.Disable,

                Permissions.OrganizationUnits.Default,
                Permissions.OrganizationUnits.Create,
                Permissions.OrganizationUnits.Update,
                Permissions.OrganizationUnits.Disable,

                Permissions.Users.Default,
                Permissions.Users.Create,
                Permissions.Users.Update,
                Permissions.Users.Disable,

                Permissions.Certificates.Default,
                Permissions.Certificates.Create,
                Permissions.Certificates.Update,
                Permissions.Certificates.Disable
            };
        }
    }
}