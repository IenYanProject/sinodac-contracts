using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateGroup(CreateGroupInput input)
        {
            AssertPermission(input.FromId, true, Permission.OrganizationGroup.Create);
            var organizationUnit = State.OrganizationUnitMap[State.UserMap[input.FromId].OrganizationName];
            Assert(organizationUnit.OrganizationName == input.OrganizationName, "不能创建其他机构的权限组");
            var groupKey = GetOrganizationGroupKey(input.OrganizationName, input.GroupName);
            foreach (var actionId in input.GroupPermissionList.Value)
            {
                Assert(State.RolePermissionMap[organizationUnit.RoleName][actionId], $"无法授予权限：{actionId}");
            }

            State.GroupActionIdListMap[groupKey] = input.GroupPermissionList;

            var organizationGroup = new OrganizationGroup
            {
                OrganizationName = input.OrganizationName,
                GroupName = input.GroupName,
                MemberList = input.GroupPermissionList
            };
            State.OrganizationGroupMap[groupKey] = organizationGroup;

            Context.Fire(new OrganizationGroupCreated
            {
                FromId = input.FromId,
                OrganizationGroup = organizationGroup
            });
            return new Empty();
        }

        public override Empty UpdateGroup(UpdateGroupInput input)
        {
            AssertPermission(input.FromId, true, Permission.OrganizationGroup.Update);
            var organizationUnit = State.OrganizationUnitMap[State.UserMap[input.FromId].OrganizationName];
            Assert(organizationUnit.OrganizationName == input.OrganizationName, "不能创建其他机构的权限组");
            var groupKey = GetOrganizationGroupKey(input.OrganizationName, input.GroupName);
            var permissionList = State.GroupActionIdListMap[groupKey];
            foreach (var actionId in input.EnableGroupPermissionList.Value)
            {
                permissionList.Value.Add(actionId);
                Assert(State.RolePermissionMap[organizationUnit.RoleName][actionId], $"无法授予权限：{actionId}");
            }

            foreach (var actionId in input.DisableGroupPermissionList.Value)
            {
                if (permissionList.Value.Contains(actionId))
                {
                    permissionList.Value.Remove(actionId);
                }
            }

            State.GroupActionIdListMap[groupKey] = permissionList;

            Context.Fire(new OrganizationGroupUpdated
            {
                FromId = input.FromId,
                OrganizationName = input.OrganizationName,
                GroupName = input.GroupName,
                UpdatedPermissionList = permissionList
            });

            return new Empty();
        }

        public override Empty DeleteGroup(DeleteGroupInput input)
        {
            AssertPermission(input.FromId, true, Permission.OrganizationGroup.Delete);
            var organizationUnit = State.OrganizationUnitMap[State.UserMap[input.FromId].OrganizationName];
            Assert(organizationUnit.OrganizationName == input.OrganizationName, "不能创建其他机构的权限组");
            var groupKey = GetOrganizationGroupKey(input.OrganizationName, input.GroupName);
            State.GroupActionIdListMap.Remove(groupKey);
            State.OrganizationGroupMap.Remove(groupKey);

            Context.Fire(new OrganizationGroupDeleted
            {
                FromId = input.FromId,
                OrganizationName = input.OrganizationName,
                GroupName = input.GroupName
            });
            return new Empty();
        }
    }
}