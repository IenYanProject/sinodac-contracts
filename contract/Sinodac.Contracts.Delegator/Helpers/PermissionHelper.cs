using System.Collections.Generic;

namespace Sinodac.Contracts.Delegator.Helpers
{
    public static class PermissionHelper
    {
        public static List<string> GetAllPermissionIdList()
        {
            return new List<string>
            {
                HomePage.Default,

                DAC.Default,
                DAC.List,
                DAC.Create,
                DAC.CreateProtocol,
                DAC.CreateSeries,
                DAC.CreateMysteryBox,
                DAC.Audit,
                DAC.AuditDetail,
                DAC.Copyright,

                Permission.Default,
                Permission.Role.RoleDefault,
                Permission.Role.Create,
                Permission.Role.Update,
                Permission.Role.Disable,
                Permission.OrganizationUnit.OrganizationUnitDefault,
                Permission.OrganizationUnit.Create,
                Permission.OrganizationUnit.Update,
                Permission.OrganizationUnit.Disable,
                Permission.OrganizationGroup.OrganizationGroupDefault,
                Permission.OrganizationGroup.Create,
                Permission.OrganizationGroup.Update,
                Permission.OrganizationGroup.Delete,
                Permission.User.UserDefault,
                Permission.User.Create,
                Permission.User.Update,
                Permission.User.Disable,
                Permission.IndependentArtist.IndependentArtistDefault,
                Permission.IndependentArtist.Create,
                Permission.IndependentArtist.Update,
                Permission.IndependentArtist.Disable,

                Statistic.Default,

                Profile.Default,
                Profile.Information,
                Profile.CertificateOrganizationUnit,
                Profile.CertificateIndependentArtist
            };
        }

        public static List<string> GetDefaultPermissionIdList()
        {
            return new List<string>
            {
                Profile.Default,
                Profile.Information,
                Profile.CertificateOrganizationUnit,
                Profile.CertificateIndependentArtist
            };
        }

        public static List<string> GetUserRelatedPermissionIdList()
        {
            return new List<string>
            {
                Permission.User.UserDefault,
                Permission.User.Create,
                Permission.User.Update,
                Permission.User.Disable
            };
        }

        public static List<string> GetMuseumPermissionIdList()
        {
            return new List<string>
            {
                HomePage.Default,

                DAC.Default,
                DAC.List,
                DAC.Create,
                DAC.CreateProtocol,
                DAC.CreateSeries,
                DAC.CreateMysteryBox,

                Permission.Default,
                Permission.OrganizationGroup.OrganizationGroupDefault,
                Permission.OrganizationGroup.Create,
                Permission.OrganizationGroup.Update,
                Permission.OrganizationGroup.Delete,
                Permission.User.UserDefault,
                Permission.User.Create,
                Permission.User.Update,
                Permission.User.Disable,

                Statistic.Default,

                Profile.Default,
                Profile.Information
            };
        }
    }
}