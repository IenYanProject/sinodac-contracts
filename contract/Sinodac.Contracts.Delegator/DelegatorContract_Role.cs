using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateRole(CreateRoleInput input)
        {
            CheckPermission(input.FromId, Permissions.Roles.Create);
            var role = new Role
            {
                RoleName = input.RoleName,
                RoleCreator = input.FromId,
                CreateTime = Context.CurrentBlockTime,
                Enabled = input.Enable,
                RoleDescription = input.RoleDescription
            };
            State.RoleMap[input.RoleName] = role;
            foreach (var permission in input.PermissionList)
            {
                State.RolePermissionMap[input.RoleName][permission] = true;
            }

            Context.Fire(new RoleCreated
            {
                Role = role
            });
            return new Empty();
        }

        public override Empty UpdateRole(UpdateRoleInput input)
        {
            CheckPermission(input.FromId, Permissions.Roles.Update);
            var oldRole = State.RoleMap[input.RoleName].Clone();
            if (oldRole.Enabled)
            {
                Assert(input.Enable, "更新角色信息时无法禁用角色");
            }
            var role = new Role
            {
                RoleName = input.RoleName,
                RoleDescription = input.RoleDescription,
                LatestEditTime = Context.CurrentBlockTime,
                // Stay old values.
                RoleCreator = oldRole.RoleCreator,
                CreateTime = oldRole.CreateTime,
                Enabled = oldRole.Enabled,
                OrganizationUnitCount = oldRole.OrganizationUnitCount,
                UserCount = oldRole.UserCount
            };
            State.RoleMap[input.RoleName] = role;
            foreach (var actionId in input.PermissionList)
            {
                State.RolePermissionMap[input.RoleName][actionId] = true;
            }

            Context.Fire(new RoleUpdated
            {
                Role = role
            });
            return new Empty();
        }

        public override Empty DisableRole(DisableRoleInput input)
        {
            CheckPermission(input.FromId, Permissions.Roles.Disable);

            if (input.Enable)
            {
                State.RoleMap[input.RoleName].Enabled = true;
                return new Empty();
            }

            var organizationUnitList = State.RoleOrganizationUnitListMap[input.RoleName];
            foreach (var organizationUnit in organizationUnitList.Value)
            {
                Assert(!State.OrganizationUnitMap[organizationUnit].Enabled,
                    $"当前角色下存在【启用】状态机构 {organizationUnit} ，请先禁用该机构后再禁用当前角色");
            }

            State.RoleMap[input.RoleName].Enabled = false;
            Context.Fire(new RoleDisabled
            {
                RoleName = input.RoleName
            });
            return new Empty();
        }

        public override RoleList GetRoleList(GetRoleListInput input)
        {
            return base.GetRoleList(input);
        }
    }
}