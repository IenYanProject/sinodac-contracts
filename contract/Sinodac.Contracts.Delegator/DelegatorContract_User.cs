using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateUser(CreateUserInput input)
        {
            AssertPermission(input.FromId, false, Permission.User.Create);

            if (string.IsNullOrEmpty(input.OrganizationName))
            {
                input.OrganizationName = DefaultOrganizationName;
            }

            var organizationUnit = State.OrganizationUnitMap[input.OrganizationName];
            if (organizationUnit == null)
            {
                throw new AssertionException($"机构 {input.OrganizationName} 不存在");
            }

            if (input.OrganizationName != DefaultOrganizationName)
            {
                Assert(organizationUnit.AdminList.Value.Contains(input.FromId),
                    $"用户 {input.FromId} 不是机构 {input.OrganizationName} 的管理员，没有权限创建用户");
            }

            Assert(organizationUnit.Enabled, $"机构 {input.OrganizationName} 当前被禁用");

            if (input.IsAdmin)
            {
                State.OrganizationUnitMap[input.OrganizationName].AdminList.Value.Add(input.UserName);
            }

            if (input.Enable)
            {
                State.OrganizationUnitMap[input.OrganizationName].UserCount = organizationUnit.UserCount.Add(1);
                State.RoleMap[organizationUnit.RoleName].UserCount =
                    State.RoleMap[organizationUnit.RoleName].UserCount.Add(1);
            }

            var groupName = input.IsAdmin ? Admin : input.GroupName ?? Member;
            Assert(State.OrganizationGroupMap[GetOrganizationGroupKey(input.OrganizationName, groupName)] != null,
                $"机构 {input.OrganizationName} 不存在 {input.GroupName} 组");

            var creatorUserInfo = State.UserMap[input.FromId];

            var user = new User
            {
                UserCreator = input.FromId,
                UserName = input.UserName,
                Enabled = input.Enable,
                OrganizationName = input.OrganizationName,
                CreateTime = Context.CurrentBlockTime,
                UserCreatorOrganizationName = creatorUserInfo.OrganizationName,
                OrganizationGroupName = groupName
            };
            State.UserMap[input.UserName] = user;
            Context.Fire(new UserCreated
            {
                FromId = input.FromId,
                User = user
            });
            return new Empty();
        }

        public override Empty UpdateUser(UpdateUserInput input)
        {
            AssertPermission(input.FromId, true, Permission.User.Update);
            Assert(State.UserMap[input.UserName].Enabled == input.Enable, "更新用户信息时无法禁用或启用用户");

            if (input.OrganizationGroupName == Admin &&
                !State.OrganizationUnitMap[input.OrganizationName].AdminList.Value.Contains(input.UserName))
            {
                State.OrganizationUnitMap[input.OrganizationName].AdminList.Value.Add(input.UserName);
            }

            TransferGroup(input.UserName, input.OrganizationName, State.UserMap[input.UserName].OrganizationGroupName,
                input.OrganizationGroupName);

            var user = new User
            {
                UserCreator = input.FromId,
                UserName = input.UserName,
                OrganizationName = input.OrganizationName,
                OrganizationGroupName = input.OrganizationGroupName,
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

        private void TransferGroup(string userName, string organizationName, string oldGroupName, string newGroupName)
        {
            
        }

        public override Empty DisableUser(DisableUserInput input)
        {
            AssertPermission(input.FromId, true, Permission.User.Disable);
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

        public override UserList GetUserList(GetUserListInput input)
        {
            return base.GetUserList(input);
        }
    }
}