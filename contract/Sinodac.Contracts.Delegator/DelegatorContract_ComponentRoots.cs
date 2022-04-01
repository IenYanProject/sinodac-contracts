using Sinodac.Contracts.Delegator.Managers;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        private RoleManager GetRoleManager()
        {
            return new RoleManager(Context, State.RoleMap, State.RolePermissionMap, State.RolePermissionListMap,
                State.RoleOrganizationUnitListMap);
        }

        private OrganizationUnitManager GetOrganizationUnitManager(RoleManager roleManager = null)
        {
            if (roleManager == null)
            {
                roleManager = GetRoleManager();
            }

            return new OrganizationUnitManager(Context, roleManager, State.OrganizationCertificateMap,
                State.OrganizationUnitMap, State.OrganizationDepartmentMap, State.OrganizationDepartmentPermissionMap,
                State.OrganizationDepartmentPermissionListMap);
        }

        private UserManager GetUserManager(RoleManager roleManager = null,
            OrganizationUnitManager organizationUnitManager = null)
        {
            if (organizationUnitManager == null)
            {
                if (roleManager == null)
                {
                    roleManager = GetRoleManager();
                }

                organizationUnitManager = GetOrganizationUnitManager(roleManager);
            }

            return new UserManager(Context, organizationUnitManager, State.UserMap);
        }
    }
}