using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Address CalculateUserAddress(StringValue input)
        {
            return GetVirtualAddress(input.Value);
        }

        public override Role GetRole(StringValue input)
        {
            return State.RoleMap[input.Value];
        }

        public override OrganizationUnit GetOrganizationUnit(StringValue input)
        {
            return State.OrganizationUnitMap[input.Value];
        }

        public override User GetUser(StringValue input)
        {
            return State.UserMap[input.Value];
        }

        public override OrganizationCertificate GetOrganizationCertificate(StringValue input)
        {
            return State.OrganizationCertificateMap[input.Value];
        }

        public override IndependentCertificate GetIndependentCertificate(StringValue input)
        {
            return State.IndependentCertificateMap[input.Value];
        }

        public override StringList GetRolePermissionList(StringValue input)
        {
            return State.RoleActionIdListMap[input.Value];
        }

        public override StringList GetOrganizationGroupPermissionList(GetOrganizationGroupPermissionListInput input)
        {
            return State.GroupActionIdListMap[GetOrganizationGroupKey(input.OrganizationName, input.GroupName)];
        }
    }
}