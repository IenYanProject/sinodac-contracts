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

        public override Empty EditRole(EditRoleInput input)
        {
            CheckPermission(input.FromId, Permissions.Roles.Edit);
            var role = new Role
            {
                RoleName = input.RoleName,
                RoleCreator = input.FromId,
                CreateTime = Context.CurrentBlockTime,
                Enabled = input.Enable,
                RoleDescription = input.RoleDescription
            };
            State.RoleMap[input.RoleName] = role;
            foreach (var actionId in input.PermissionList)
            {
                State.RolePermissionMap[input.RoleName][actionId] = true;
            }

            Context.Fire(new RoleEdited
            {
                Role = role
            });
            return new Empty();
        }

        public override Empty DeleteRole(DeleteRoleInput input)
        {
            CheckPermission(input.FromId, Permissions.Roles.Edit);
            var organizationUnitList = State.RoleOrganizationUnitListMap[input.RoleName];
            foreach (var organizationUnit in organizationUnitList.Value)
            {
                var userList = State.OrganizationUnitUserListMap[organizationUnit];
                foreach (var user in userList.Value)
                {
                    Assert(!State.UserMap[user].Enabled, "当前角色下存在【启用】状态用户，请先禁用该用户后再禁用当前角色");
                }
            }

            Context.Fire(new RoleDeleted
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