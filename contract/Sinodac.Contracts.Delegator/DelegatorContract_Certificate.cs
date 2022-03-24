using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateOrganizationCertificate(CreateOrganizationCertificateInput input)
        {
            CheckPermissions(input.FromId, Profile.CertificateOrganizationUnit);
            var organizationCertificate = new OrganizationCertificate
            {
                CreateTime = Context.CurrentBlockTime,
                OrganizationDescription = input.OrganizationDescription,
                OrganizationEmail = input.OrganizationEmail,
                OrganizationLevel = input.OrganizationLevel,
                OrganizationLocation = input.OrganizationLocation,
                OrganizationName = input.OrganizationName,
                OrganizationType = input.OrganizationType,
                OrganizationArtificialPerson = input.OrganizationArtificialPerson,
                OrganizationCreditCode = input.OrganizationCreditCode,
                OrganizationEstablishedTime = input.OrganizationEstablishedTime,
                OrganizationPhoneNumber = input.OrganizationPhoneNumber,
                RegistrationAuthority = input.RegistrationAuthority,
                PhotoIds = { input.PhotoIds }
            };
            State.OrganizationCertificateMap[input.OrganizationName] = organizationCertificate;
            
            return new Empty();
        }

        public override Empty UpdateOrganizationCertificate(UpdateOrganizationCertificateInput input)
        {
            CheckPermissions(input.FromId, Profile.CertificateOrganizationUnit);
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