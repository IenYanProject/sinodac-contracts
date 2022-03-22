using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateOrganizationUnit(CreateOrganizationUnitInput input)
        {
            return base.CreateOrganizationUnit(input);
        }

        public override Empty EditOrganizationUnit(EditOrganizationUnitInput input)
        {
            return base.EditOrganizationUnit(input);
        }

        public override Empty DeleteOrganizationUnit(DeleteOrganizationUnitInput input)
        {
            return base.DeleteOrganizationUnit(input);
        }

        public override OrganizationUnitList GetOrganizationUnitList(GetOrganizationUnitListInput input)
        {
            return base.GetOrganizationUnitList(input);
        }
    }
}