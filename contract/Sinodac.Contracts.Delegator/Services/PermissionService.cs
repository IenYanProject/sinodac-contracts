using Sinodac.Contracts.Delegator.Managers;

namespace Sinodac.Contracts.Delegator.Services
{
    public class PermissionService
    {
        private readonly RoleManager _roleManager;
        private readonly OrganizationUnitManager _organizationUnitManager;
        private readonly IndependentArtistManager _independentArtistManager;
        private readonly UserManager _userManager;

        public PermissionService(RoleManager roleManager, OrganizationUnitManager organizationUnitManager,
            IndependentArtistManager independentArtistManager, UserManager userManager)
        {
            _roleManager = roleManager;
            _organizationUnitManager = organizationUnitManager;
            _independentArtistManager = independentArtistManager;
            _userManager = userManager;
        }
    }
}