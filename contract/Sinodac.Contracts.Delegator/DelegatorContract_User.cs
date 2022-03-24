using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateUser(CreateUserInput input)
        {
            CheckPermissions(input.FromId, Permission.User.Create);

            var organizationUnit = State.OrganizationUnitMap[input.OrganizationName];
            if (organizationUnit == null)
            {
                throw new AssertionException($"机构 {input.OrganizationName} 不存在");
            }

            Assert(organizationUnit.Enabled, $"机构 {input.OrganizationName} 当前被禁用");

            State.OrganizationUnitMap[input.OrganizationName].UserCount = organizationUnit.UserCount.Add(1);
            State.RoleMap[organizationUnit.RoleName].UserCount =
                State.RoleMap[organizationUnit.RoleName].UserCount.Add(1);

            var user = new User
            {
                UserCreator = input.FromId,
                UserName = input.UserName,
                Enabled = input.Enable,
                OrganizationName = input.OrganizationName,
                CreateTime = Context.CurrentBlockTime
            };
            State.UserMap[input.UserName] = user;
            Context.Fire(new UserCreated
            {
                User = user
            });
            return new Empty();
        }

        public override Empty UpdateUser(UpdateUserInput input)
        {
            CheckPermissions(input.FromId, Permission.User.Update);
            Assert(State.UserMap[input.UserName].Enabled == input.Enable, "更新用户信息时无法禁用或启用用户");

            var user = new User
            {
                UserCreator = input.FromId,
                UserName = input.UserName,
                OrganizationName = input.OrganizationName,
                // Stay old values.
                Enabled = State.UserMap[input.UserName].Enabled,
                CreateTime = State.UserMap[input.UserName].CreateTime
            };
            State.UserMap[input.UserName] = user;
            Context.Fire(new UserUpdated
            {
                User = user
            });
            return new Empty();
        }

        public override Empty DisableUser(DisableUserInput input)
        {
            CheckPermissions(input.FromId, Permission.User.Disable);
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
                UserName = input.UserName
            });
            return new Empty();
        }

        public override UserList GetUserList(GetUserListInput input)
        {
            return base.GetUserList(input);
        }
    }
}