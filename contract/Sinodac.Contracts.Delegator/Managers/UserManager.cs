using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;

namespace Sinodac.Contracts.Delegator.Managers
{
    public class UserManager
    {
        private readonly CSharpSmartContractContext _context;
        private readonly OrganizationUnitManager _organizationUnitManager;
        private readonly MappedState<string, User> _userMap;

        public UserManager(CSharpSmartContractContext context, OrganizationUnitManager organizationUnitManager,
            MappedState<string, User> userMap)
        {
            _context = context;
            _organizationUnitManager = organizationUnitManager;
            _userMap = userMap;
        }

        public void Initialize()
        {
            var adminUserName = nameof(DelegatorContractConstants.Admin).ToLower();
            AddUser(new User
            {
                CreateTime = _context.CurrentBlockTime,
                Enabled = true,
                OrganizationName = DelegatorContractConstants.Admin,
                UserCreator = DelegatorContractConstants.System,
                UserName = adminUserName,
                OrganizationDepartmentName = DelegatorContractConstants.Admin
            });
        }

        public void AddUser(User user)
        {
            if (_userMap[user.UserName] != null)
            {
                throw new AssertionException($"用户 {user.UserName} 已经存在了");
            }

            _userMap[user.UserName] = user;
            _organizationUnitManager.AddUser(user);

            _context.Fire(new UserCreated
            {
                FromId = user.UserCreator,
                User = user
            });
        }
    }
}