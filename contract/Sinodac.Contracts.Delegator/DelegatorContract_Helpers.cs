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
            var user = userManager.GetUser(fromId);
            var organizationUnit = organizationUnitManager.GetOrganizationUnit(user.OrganizationName);

            Assert(user.Enabled, $"用户 {fromId} 当前为禁用状态");
            Assert(actionIds.Any(actionId => CheckPermission(user, organizationUnit.RoleName, actionId)),
                $"用户 {fromId} 没有权限调用当前方法");

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
            var departmentKey =
                KeyHelper.GetOrganizationDepartmentKey(user.OrganizationName, user.OrganizationDepartmentName);
            Assert(
                !State.OrganizationDepartmentIgnoredPermissionListMap[departmentKey].Value.Contains(actionId),
                $"{user.UserName} 所属部门 {departmentKey} 无权调用当前方法：无 {actionId} 权限");
            Assert(
                State.RolePermissionMap[roleName][actionId],
                $"{user.UserName} 所属机构的角色 {roleName} 无权调用当前方法：无 {actionId} 权限");
            return true;
        }

        private void TransferPermissionToForwardPermission(List<string> actionIds)
        {
            var dacContractActionIds = new List<string>
            {
                DAC.CreateProtocol,
                DAC.CreateSeries,
                DAC.CreateMysteryBox,
            };
            var dacMarketContractActionIds = new List<string>
            {
                DAC.List
            };

            // TODO: Move this logic to DAC Contract & DAC Market Contract.
        }

        private List<string> GetAllActionIds()
        {
            return new List<string>
            {
                HomePage.Default,

                DAC.Default,
                DAC.List,
                DAC.Create,
                DAC.CreateProtocol,
                DAC.CreateSeries,
                DAC.CreateMysteryBox,
                DAC.Audit,
                DAC.AuditDetail,
                DAC.Copyright,

                Permission.Default,
                Permission.Role.RoleDefault,
                Permission.Role.Create,
                Permission.Role.Update,
                Permission.Role.Disable,
                Permission.OrganizationUnit.OrganizationUnitDefault,
                Permission.OrganizationUnit.Create,
                Permission.OrganizationUnit.Update,
                Permission.OrganizationUnit.Disable,
                Permission.User.UserDefault,
                Permission.User.Create,
                Permission.User.Update,
                Permission.User.Disable,
                Permission.IndependentArtist.IndependentArtistDefault,
                Permission.IndependentArtist.Create,
                Permission.IndependentArtist.Update,
                Permission.IndependentArtist.Disable,

                Statistic.Default,

                Profile.Default,
                Profile.Information,
                Profile.CertificateOrganizationUnit,
                Profile.CertificateIndependentArtist
            };
        }

        private Address GetVirtualAddress(string fromId)
        {
            return Context.ConvertVirtualAddressToContractAddress(HashHelper.ComputeFrom(fromId));
        }

        private void SetDefaultPermissionsToDefaultRole()
        {
            State.RolePermissionListMap[DefaultRoleName] = new StringList
            {
                Value =
                {
                    Profile.Default,
                    Profile.Information,
                    Profile.CertificateOrganizationUnit,
                    Profile.CertificateIndependentArtist
                }
            };
            foreach (var actionId in State.RolePermissionListMap[DefaultRoleName].Value)
            {
                State.RolePermissionMap[DefaultRoleName][actionId] = true;
            }
        }

        private string GetOrganizationAdminKey(string organizationName)
        {
            return $"{organizationName}-管理员";
        }

        private string GetOrganizationMemberKey(string organizationName)
        {
            return $"{organizationName}-员工";
        }

        private string GetOrganizationDepartmentKey(string organizationName, string groupName)
        {
            return $"{organizationName}-{groupName}";
        }
    }
}