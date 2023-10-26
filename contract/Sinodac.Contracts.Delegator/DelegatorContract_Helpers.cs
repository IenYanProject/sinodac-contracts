using System.Collections.Generic;
using System.Linq;
using AElf;
using AElf.Sdk.CSharp;
using AElf.Types;
using Sinodac.Contracts.Delegator.Helpers;
using Sinodac.Contracts.Delegator.Managers;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        private class ManagerList
        {
            public RoleManager RoleManager { get; set; }
            public OrganizationUnitManager OrganizationUnitManager { get; set; }
            public UserManager UserManager { get; set; }
            public IndependentArtistManager IndependentArtistManager { get; set; }
        }

        private ManagerList AssertPermission(string fromId, params string[] actionIds)
        {
            var organizationUnitManager = GetOrganizationUnitManager();
            var roleManager = GetRoleManager();
            var userManager = GetUserManager(roleManager, organizationUnitManager);

            if (State.EnablePermissionCheck.Value)
            {
                var user = userManager.GetUser(fromId);
                var organizationUnit = organizationUnitManager.GetOrganizationUnit(user.OrganizationName);
                Assert(user.Enabled, $"用户 {fromId} 当前为禁用状态");
                Assert(actionIds.Any(actionId => CheckPermission(user, organizationUnit.RoleName, actionId)),
                    $"用户 {fromId} 没有权限调用当前方法");
            }

            return new ManagerList
            {
                RoleManager = roleManager,
                OrganizationUnitManager = organizationUnitManager,
                UserManager = userManager,
                IndependentArtistManager = GetIndependentArtistManager()
            };
        }

        /// <summary>
        /// Only AssertPermission method should call this method.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        /// <exception cref="AssertionException"></exception>
        private bool CheckPermission(User user, string roleName, string actionId)
        {
            // var departmentKey =
            //     KeyHelper.GetOrganizationDepartmentKey(user.OrganizationName, user.OrganizationDepartmentName);
            // Assert(
            //     !State.OrganizationDepartmentIgnoredPermissionListMap[departmentKey].Value.Contains(actionId),
            //     $"{user.UserName} 所属部门 {departmentKey} 无权调用当前方法：无 {actionId} 权限");
            Assert(
                State.RolePermissionMap[roleName][actionId],
                $"{user.UserName} 所属机构的角色 {roleName} 无权调用当前方法：无 {actionId} 权限");
            return true;
        }

        private Address GetVirtualAddress(string fromId)
        {
            return Context.ConvertVirtualAddressToContractAddress(HashHelper.ComputeFrom(fromId));
        }

        private string GetOrganizationDepartmentKey(string organizationName, string groupName)
        {
            return $"{organizationName}-{groupName}";
        }
    }
}