using System.Collections.Generic;
using System.Linq;
using AElf;
using AElf.Sdk.CSharp;
using AElf.Types;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        private void AssertPermission(string fromId, bool isNeedToBeOrganizationAdmin = false,
            params string[] actionIds)
        {
            Assert(actionIds.Any(actionId => CheckPermission(fromId, actionId, isNeedToBeOrganizationAdmin)),
                $"用户{fromId}没有权限调用当前方法");
        }

        /// <summary>
        /// Only AssertPermission method should use this method.
        /// </summary>
        /// <param name="fromId"></param>
        /// <param name="actionId"></param>
        /// <param name="isNeedToBeOrganizationAdmin"></param>
        /// <returns></returns>
        /// <exception cref="AssertionException"></exception>
        private bool CheckPermission(string fromId, string actionId, bool isNeedToBeOrganizationAdmin)
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

            if (isNeedToBeOrganizationAdmin)
            {
                Assert(organizationUnit.AdminList.Value.Contains(fromId), $"{user} 不是 {user.OrganizationName} 的管理员");
            }

            Assert(organizationUnit.Enabled, $"机构 {organizationUnit.OrganizationName} 当前为禁用状态");

            return State.RolePermissionMap[organizationUnit.RoleName][actionId];
        }

        private void TransferPermissionToForwardPermission(List<string> actionIds)
        {
            var dacContractActionIds = new List<string>
            {
                DAC.CreateProtocol,
                DAC.CreateCollection,
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
                DAC.CreateCollection,
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
    }
}