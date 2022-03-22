using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateCertificate(CreateCertificateInput input)
        {
            return base.CreateCertificate(input);
        }

        public override Empty EditCertificate(EditCertificateInput input)
        {
            return base.EditCertificate(input);
        }

        public override Empty DeleteCertificate(DeleteCertificateInput input)
        {
            return base.DeleteCertificate(input);
        }

        public override CertificateList GetCertificateList(GetCertificateListInput input)
        {
            return base.GetCertificateList(input);
        }
    }
}