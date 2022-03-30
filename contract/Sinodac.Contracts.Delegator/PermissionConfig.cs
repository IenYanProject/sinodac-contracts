namespace Sinodac.Contracts.Delegator
{
    public static class HomePage
    {
        public const string Default = nameof(HomePage);
    }

    // ReSharper disable once InconsistentNaming
    public static class DAC
    {
        public const string Default = nameof(DAC);
        public const string List = Default + ":List";
        public const string Create = Default + ":Create";
        public const string CreateCollection = Create + ":Collection";
        public const string CreateSeries = Create + ":Series";
        public const string CreateMysteryBox = Create + ":MysteryBox";

        public const string Audit = Default + ":Audit";
        public const string AuditDetail = Audit + ":Detail";

        public const string Copyright = Default + ":Copyright";
    }

    public static class Permission
    {
        public const string Default = nameof(Permission);

        public class Role
        {
            public const string RoleDefault = Default + ":Role";
            public const string Create = RoleDefault + ":Create";
            public const string Update = RoleDefault + ":Update";
            public const string Disable = RoleDefault + ":Disable";
        }

        public class OrganizationUnit
        {
            public const string OrganizationUnitDefault = Default + ":OrganizationUnit";
            public const string Create = OrganizationUnitDefault + ":Create";
            public const string Update = OrganizationUnitDefault + ":Update";
            public const string Disable = OrganizationUnitDefault + ":Disable";
        }

        public class User
        {
            public const string UserDefault = nameof(User);
            public const string Create = UserDefault + ":Create";
            public const string Update = UserDefault + ":Update";
            public const string Disable = UserDefault + ":Disable";
        }

        public class IndependentArtist
        {
            public const string IndependentArtistDefault = Default + ":IndependentArtist";
            public const string Create = IndependentArtistDefault + ":Create";
            public const string Update = IndependentArtistDefault + ":Update";
            public const string Disable = IndependentArtistDefault + ":Disable";
        }
    }

    public static class Statistic
    {
        public const string Default = nameof(Statistic);
    }

    public static class Profile
    {
        public const string Default = nameof(Profile);
        public const string Information = Default + ":Information";
        public const string CertificateOrganizationUnit = Default + ":Certificate:OrganizationUnit";
        public const string CertificateIndependentArtist = Default + ":Certificate:IndependentArtist";
    }
}