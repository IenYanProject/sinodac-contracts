using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateOrganizationUnit(CreateOrganizationUnitInput input)
        {
            CheckPermissions(input.FromId, Profile.CertificateOrganizationUnit, Permission.OrganizationUnit.Create);

            var role = State.RoleMap[input.RoleName];
            if (role == null)
            {
                throw new AssertionException($"角色 {input.RoleName} 不存在");
            }

            Assert(role.Enabled, $"角色 {input.RoleName} 当前被禁用");

            State.RoleMap[input.RoleName].OrganizationUnitCount = role.OrganizationUnitCount.Add(1);

            var organizationUnit = new OrganizationUnit
            {
                OrganizationName = input.OrganizationName,
                OrganizationCreator = input.FromId,
                Enabled = input.Enable,
                RoleName = input.RoleName,
                CreateTime = Context.CurrentBlockTime
            };
            State.OrganizationUnitMap[input.OrganizationName] = organizationUnit;

            Context.Fire(new OrganizationUnitCreated
            {
                OrganizationUnit = organizationUnit
            });
            return new Empty();
        }

        public override Empty UpdateOrganizationUnit(UpdateOrganizationUnitInput input)
        {
            CheckPermissions(input.FromId, Profile.CertificateOrganizationUnit, Permission.OrganizationUnit.Update);

            var oldOrganizationUnit = State.OrganizationUnitMap[input.OrganizationName].Clone();
            Assert(oldOrganizationUnit.Enabled == input.Enable, "更新机构信息时无法禁用或启用机构");

            var organizationUnit = new OrganizationUnit
            {
                OrganizationName = input.OrganizationName,
                RoleName = input.RoleName,
                // Stay old values.
                OrganizationCreator = oldOrganizationUnit.OrganizationCreator,
                Enabled = oldOrganizationUnit.Enabled,
                CreateTime = oldOrganizationUnit.CreateTime,
                UserCount = oldOrganizationUnit.UserCount
            };
            State.OrganizationUnitMap[input.OrganizationName] = organizationUnit;
            Context.Fire(new OrganizationUnitUpdated
            {
                OrganizationUnit = organizationUnit
            });
            return new Empty();
        }

        public override Empty DisableOrganizationUnit(DisableOrganizationUnitInput input)
        {
            CheckPermissions(input.FromId, Permission.OrganizationUnit.Disable);

            var organizationUnit = State.OrganizationUnitMap[input.OrganizationName];

            if (input.Enable)
            {
                State.OrganizationUnitMap[input.OrganizationName].Enabled = true;
                Assert(State.RoleMap[organizationUnit.RoleName].Enabled,
                    $"机构 {input.OrganizationName} 所属角色 {organizationUnit.RoleName} 当前为禁用状态");
                State.RoleMap[organizationUnit.RoleName].OrganizationUnitCount =
                    State.RoleMap[organizationUnit.RoleName].OrganizationUnitCount.Add(1);
                Context.Fire(new OrganizationUnitEnabled
                {
                    OrganizationName = input.OrganizationName
                });
                return new Empty();
            }

            var userList = State.OrganizationUnitUserListMap[input.OrganizationName];
            foreach (var user in userList.Value)
            {
                Assert(!State.UserMap[user].Enabled, $"当前角色下存在【启用】状态用户 {user} ，请先禁用该用户后再禁用当前角色");
            }

            State.OrganizationUnitMap[input.OrganizationName].Enabled = false;

            State.RoleMap[organizationUnit.RoleName].OrganizationUnitCount =
                State.RoleMap[organizationUnit.RoleName].OrganizationUnitCount.Sub(1);

            Context.Fire(new OrganizationUnitDisabled
            {
                OrganizationName = input.OrganizationName
            });
            return new Empty();
        }

        public override OrganizationUnitList GetOrganizationUnitList(GetOrganizationUnitListInput input)
        {
            return base.GetOrganizationUnitList(input);
        }
    }
}