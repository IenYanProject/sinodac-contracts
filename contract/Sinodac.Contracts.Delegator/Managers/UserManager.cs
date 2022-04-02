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

        public void UpdateUser(User user)
        {
            var previousUser = GetUser(user.UserName);

            if (previousUser.OrganizationName != user.OrganizationName)
            {
                // 这种情况只是调整这个用户的所属机构，步骤：
                // 1. 用户原本的机构和角色减员1
                // 2. 用户新的机构和角色增员1，添加进部门人员
                // 3. 覆盖user
                _organizationUnitManager.SubUserCount(previousUser.OrganizationName);
                _organizationUnitManager.AddUser(user);
                _userMap[user.UserName] = user;
            }
            else
            {
                _userMap[user.UserName] = user;
            }
        }

        public void DisableUser(string userName)
        {
            var user = GetUser(userName);
            if (user.Enabled)
            {
                throw new AssertionException($"用户 {user.UserName} 当前为可用状态");
            }

            _userMap[userName].Enabled = false;

            _organizationUnitManager.SubUserCount(user.OrganizationName);
        }

        public User GetUser(string userName)
        {
            AssertUserExists(userName);
            return _userMap[userName];
        }

        public void AssertUserExists(string userName)
        {
            if (_userMap[userName] == null)
            {
                throw new AssertionException($"用户 {userName} 不存在");
            }
        }
        
        public void TransferApplierToNewOrganizationUnit(string organizationName)
        {
            var organizationCertificate = _organizationUnitManager.GetOrganizationCertificate(organizationName);
            var applierName = organizationCertificate.Applier;
            var applier = _userMap[applierName];
        }
    }
}