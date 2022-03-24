using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateOrganizationCertificate(CreateOrganizationCertificateInput input)
        {
            CheckPermission(input.FromId, Permissions.Certificates.Create);
            return new Empty();
        }

        public override Empty UpdateOrganizationCertificate(UpdateOrganizationCertificateInput input)
        {
            return new Empty();
        }

        public override Empty CreateIndependentCertificate(CreateIndependentCertificateInput input)
        {
            return new Empty();
        }

        public override Empty UpdateIndependentCertificate(UpdateIndependentCertificateInput input)
        {
            return new Empty();
        }

        public override Empty DeleteCertificate(DeleteCertificateInput input)
        {
            return new Empty();
        }

        public override CertificateList GetCertificateList(GetCertificateListInput input)
        {
            return base.GetCertificateList(input);
        }
    }
}