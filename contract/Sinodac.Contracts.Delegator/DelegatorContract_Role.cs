using System.Linq;
using AElf.Sdk.CSharp;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateRole(CreateRoleInput input)
        {
            var managerList = AssertPermission(input.FromId, Permission.Role.Create);
            managerList.RoleManager.AddRole(new Role
            {
                RoleName = input.RoleName,
                RoleCreator = input.FromId,
                Enabled = input.Enable,
                RoleDescription = input.RoleDescription
            });
            managerList.RoleManager.InitialRolePermissionList(input.RoleName, input.PermissionList.ToList());
            return new Empty();
        }

        public override Empty UpdateRole(UpdateRoleInput input)
        {
            var managerList = AssertPermission(input.FromId, Permission.Role.Update);
            var oldRole = State.RoleMap[input.RoleName].Clone();
            var role = new Role
            {
                RoleName = input.RoleName,
                RoleDescription = input.RoleDescription,
                LatestEditTime = Context.CurrentBlockTime,
                LatestEditId = input.FromId,
                // Stay old values.
                RoleCreator = oldRole.RoleCreator,
                CreateTime = oldRole.CreateTime,
                Enabled = oldRole.Enabled,
                OrganizationUnitCount = oldRole.OrganizationUnitCount,
                UserCount = oldRole.UserCount
            };
            managerList.RoleManager.UpdateRole(role);
            managerList.RoleManager.UpdateRolePermissionList(role.RoleName,
                input.EnablePermissionList ?? new RepeatedField<string>(),
                input.DisablePermissionList ?? new RepeatedField<string>());
            return new Empty();
        }

        public override Empty DisableRole(DisableRoleInput input)
        {
            var managerList = AssertPermission(input.FromId, Permission.Role.Disable);
            var role = State.RoleMap[input.RoleName];
            role.Enabled = input.Enable;
            role.LatestEditTime = Context.CurrentBlockTime;
            role.LatestEditId = input.FromId;

            if (input.Enable)
            {
                managerList.RoleManager.EnableRole(role);
            }
            else
            {
                foreach (var organizationUnit in State.RoleOrganizationUnitListMap[input.RoleName].Value)
                {
                    Assert(!State.OrganizationUnitMap[organizationUnit].Enabled,
                        $"当前角色下存在【启用】状态机构 {organizationUnit} ，请先禁用该机构后再禁用当前角色");
                }

                managerList.RoleManager.DisableRole(role);
            }

            return new Empty();
        }
    }
}