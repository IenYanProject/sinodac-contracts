using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateDepartment(CreateDepartmentInput input)
        {
            AssertPermission(input.FromId, Permission.OrganizationGroup.Create);
            var organizationUnit = State.OrganizationUnitMap[State.UserMap[input.FromId].OrganizationName];
            Assert(organizationUnit.OrganizationName == input.OrganizationName, "不能创建其他机构的权限组");
            var departmentKey = GetOrganizationDepartmentKey(input.OrganizationName, input.DepartmentName);
            foreach (var actionId in input.DepartmentPermissionList.Value)
            {
                Assert(State.RolePermissionMap[organizationUnit.RoleName][actionId], $"无法授予权限：{actionId}");
            }

            State.OrganizationDepartmentIgnoredPermissionListMap[departmentKey] = input.DepartmentPermissionList;

            var organizationDepartment = new OrganizationDepartment
            {
                OrganizationName = input.OrganizationName,
                DepartmentName = input.DepartmentName,
                MemberList = input.DepartmentPermissionList
            };
            State.OrganizationDepartmentMap[departmentKey] = organizationDepartment;

            Context.Fire(new OrganizationDepartmentCreated
            {
                FromId = input.FromId,
                OrganizationDepartment = organizationDepartment
            });
            return new Empty();
        }

        public override Empty UpdateDepartment(UpdateDepartmentInput input)
        {
            AssertPermission(input.FromId, Permission.OrganizationGroup.Update);
            var organizationUnit = State.OrganizationUnitMap[State.UserMap[input.FromId].OrganizationName];
            Assert(organizationUnit.OrganizationName == input.OrganizationName, "不能创建其他机构的权限组");
            var departmentKey = GetOrganizationDepartmentKey(input.OrganizationName, input.DepartmentName);
            var permissionList = State.OrganizationDepartmentIgnoredPermissionListMap[departmentKey];
            foreach (var actionId in input.EnableDepartmentPermissionList.Value)
            {
                permissionList.Value.Add(actionId);
                Assert(State.RolePermissionMap[organizationUnit.RoleName][actionId], $"无法授予权限：{actionId}");
            }

            foreach (var actionId in input.DisableDepartmentPermissionList.Value)
            {
                if (permissionList.Value.Contains(actionId))
                {
                    permissionList.Value.Remove(actionId);
                }
            }

            State.OrganizationDepartmentIgnoredPermissionListMap[departmentKey] = permissionList;

            Context.Fire(new OrganizationDepartmentUpdated()
            {
                FromId = input.FromId,
                OrganizationName = input.OrganizationName,
                DepartmentName = input.DepartmentName,
                UpdatedPermissionList = permissionList
            });

            return new Empty();
        }

        public override Empty DeleteDepartment(DeleteDepartmentInput input)
        {
            AssertPermission(input.FromId, Permission.OrganizationGroup.Delete);
            var organizationUnit = State.OrganizationUnitMap[State.UserMap[input.FromId].OrganizationName];
            Assert(organizationUnit.OrganizationName == input.OrganizationName, "不能创建其他机构的权限组");
            var departmentKey = GetOrganizationDepartmentKey(input.OrganizationName, input.DepartmentName);
            State.OrganizationDepartmentIgnoredPermissionListMap.Remove(departmentKey);
            State.OrganizationDepartmentMap.Remove(departmentKey);

            Context.Fire(new OrganizationDepartmentDeleted
            {
                FromId = input.FromId,
                OrganizationName = input.OrganizationName,
                DepartmentName = input.DepartmentName
            });
            return new Empty();
        }
    }
}