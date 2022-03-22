namespace Sinodac.Contracts.Delegator
{
    public static class Permissions
    {
        public class HomePage
        {
            public const string Default = nameof(HomePage);
        }

        public class DAC
        {
            public const string Default = nameof(DAC);
            public const string Create = Default + ":Create";
            public const string CreateSeries = Default + ":CreateSeries";
            public const string Audit = Default + ":Audit";
        }

        public class Copyright
        {
            public const string Default = nameof(Copyright);
        }

        public class Roles
        {
            public const string Default = nameof(Roles);
            public const string Edit = Default + ":Edit";
            public const string Create = Default + ":Create";
            public const string Delete = Default + ":Delete";
        }

        public class OrganizationUnits
        {
            public const string Default = nameof(OrganizationUnits);
            public const string Edit = Default + ":Edit";
            public const string Create = Default + ":Create";
            public const string Delete = Default + ":Delete";
        }

        public class Certificates
        {
            public const string Default = nameof(Certificates);
            public const string Edit = Default + ":Edit";
            public const string Create = Default + ":Create";
            public const string Delete = Default + ":Delete";
        }

        public class Users
        {
            public const string Default = nameof(Users);
            public const string Edit = Default + ":Edit";
            public const string Create = Default + ":Create";
            public const string Delete = Default + ":Delete";
        }
    }
}