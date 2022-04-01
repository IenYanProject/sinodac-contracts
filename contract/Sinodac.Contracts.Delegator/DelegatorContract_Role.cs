using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;
using Sinodac.Contracts.Delegator.Managers;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateRole(CreateRoleInput input)
        {
            AssertPermission(input.FromId, false, Permission.Role.Create);
            GetRoleManager().AddRole(new Role
            {
                RoleName = input.RoleName,
                RoleCreator = input.FromId,
                Enabled = input.Enable,
                RoleDescription = input.RoleDescription
            });
            return new Empty();
        }

        public override Empty UpdateRole(UpdateRoleInput input)
        {
            AssertPermission(input.FromId, false, Permission.Role.Update);
            var oldRole = State.RoleMap[input.RoleName].Clone();

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
            var previewsActionIdList = State.RolePermissionListMap[input.RoleName];
            foreach (var actionId in input.EnablePermissionList)
            {
                State.RolePermissionMap[input.RoleName][actionId] = true;
                if (!previewsActionIdList.Value.Contains(actionId))
                {
                    previewsActionIdList.Value.Add(actionId);
                }
            }
            foreach (var actionId in input.DisablePermissionList)
            {
                State.RolePermissionMap[input.RoleName].Remove(actionId);
                if (previewsActionIdList.Value.Contains(actionId))
                {
                    previewsActionIdList.Value.Remove(actionId);
                }
            }

            State.RolePermissionListMap[input.RoleName] = previewsActionIdList;

            Context.Fire(new RoleUpdated
            {
                FromId = input.FromId,
                Role = role
            });
            return new Empty();
        }

        public override Empty DisableRole(DisableRoleInput input)
        {
            AssertPermission(input.FromId, false, Permission.Role.Disable);

            if (input.Enable)
            {
                State.RoleMap[input.RoleName].Enabled = true;
                Context.Fire(new RoleEnabled
                {
                    FromId = input.FromId,
                    RoleName = input.RoleName
                });
                return new Empty();
            }

            foreach (var organizationUnit in State.RoleOrganizationUnitListMap[input.RoleName].Value)
            {
                Assert(!State.OrganizationUnitMap[organizationUnit].Enabled,
                    $"当前角色下存在【启用】状态机构 {organizationUnit} ，请先禁用该机构后再禁用当前角色");
            }

            State.RoleMap[input.RoleName].Enabled = false;
            Context.Fire(new RoleDisabled
            {
                FromId = input.FromId,
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