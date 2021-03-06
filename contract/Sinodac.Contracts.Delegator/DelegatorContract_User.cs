using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;
using Sinodac.Contracts.Delegator.Helpers;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateUser(CreateUserInput input)
        {
            var managerList = AssertPermission(input.FromId, Permission.User.Create);

            if (string.IsNullOrEmpty(input.OrganizationName))
            {
                input.OrganizationName = DefaultOrganizationName;
            }

            if (string.IsNullOrEmpty(input.DepartmentName))
            {
                input.DepartmentName = Member;
            }

            // 管理员可创建自己机构的用户，也可以创建默认机构的用户
            var creator = managerList.UserManager.GetUser(input.FromId);
            Assert(
                input.OrganizationName == creator.OrganizationName || input.OrganizationName == DefaultOrganizationName,
                $"用户 {input.FromId} 无法创建机构 {input.OrganizationName} 的新用户");

            var organizationUnit = managerList.OrganizationUnitManager.GetOrganizationUnit(input.OrganizationName);
            // 只有默认机构被禁用了才会失败
            Assert(organizationUnit.Enabled, $"机构 {input.OrganizationName} 当前被禁用");

            managerList.UserManager.AddUser(new User
            {
                UserCreator = input.FromId,
                UserName = input.UserName,
                Enabled = input.Enable,
                OrganizationName = input.OrganizationName,
                UserCreatorOrganizationName = creator.OrganizationName,
                OrganizationDepartmentName = input.DepartmentName,
                RoleName = organizationUnit.RoleName
            });
            return new Empty();
        }

        public override Empty UpdateUser(UpdateUserInput input)
        {
            AssertPermission(input.FromId, Permission.User.Update);
            Assert(State.UserMap[input.UserName].Enabled == input.Enable, "更新用户信息时无法禁用或启用用户");

            if (input.OrganizationDepartmentName == Admin &&
                !State.OrganizationDepartmentMap[
                    KeyHelper.GetOrganizationDepartmentKey(input.OrganizationName,
                        DelegatorContractConstants.Admin)].MemberList.Value.Contains(input.UserName))
            {
                State.OrganizationDepartmentMap[
                    KeyHelper.GetOrganizationDepartmentKey(input.OrganizationName,
                        DelegatorContractConstants.Admin)].MemberList.Value.Add(input.UserName);
            }

            var user = new User
            {
                UserCreator = input.FromId,
                UserName = input.UserName,
                OrganizationName = input.OrganizationName,
                OrganizationDepartmentName = input.OrganizationDepartmentName,
                // Stay old values.
                Enabled = State.UserMap[input.UserName].Enabled,
                CreateTime = State.UserMap[input.UserName].CreateTime,
                UserCreatorOrganizationName = State.UserMap[input.UserName].UserCreatorOrganizationName
            };
            State.UserMap[input.UserName] = user;
            Context.Fire(new UserUpdated
            {
                FromId = input.FromId,
                User = user
            });
            return new Empty();
        }

        public override Empty DisableUser(DisableUserInput input)
        {
            AssertPermission(input.FromId, Permission.User.Disable);
            var user = State.UserMap[input.UserName];
            var organizationUnit = State.OrganizationUnitMap[user.OrganizationName];

            if (input.Enable)
            {
                State.UserMap[input.UserName].Enabled = true;
                Assert(State.OrganizationUnitMap[user.OrganizationName].Enabled,
                    $"用户 {input.UserName} 所属机构 {user.OrganizationName} 当前为禁用状态");
                State.OrganizationUnitMap[user.OrganizationName].UserCount = organizationUnit.UserCount.Add(1);
                State.RoleMap[organizationUnit.RoleName].UserCount =
                    State.RoleMap[organizationUnit.RoleName].UserCount.Add(1);
                Context.Fire(new UserEnabled
                {
                    FromId = input.FromId,
                    UserName = input.UserName
                });
                return new Empty();
            }

            State.UserMap[input.UserName].Enabled = false;

            State.OrganizationUnitMap[user.OrganizationName].UserCount =
                State.OrganizationUnitMap[user.OrganizationName].UserCount.Sub(1);

            State.RoleMap[organizationUnit.RoleName].UserCount =
                State.RoleMap[organizationUnit.RoleName].UserCount.Sub(1);

            Context.Fire(new UserDisabled
            {
                FromId = input.FromId,
                UserName = input.UserName
            });
            return new Empty();
        }

    }
}