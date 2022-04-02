using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;
using Sinodac.Contracts.Delegator.Helpers;
using Sinodac.Contracts.Delegator.Strategies;

namespace Sinodac.Contracts.Delegator.Managers
{
    public class OrganizationUnitManager
    {
        private readonly CSharpSmartContractContext _context;
        private readonly RoleManager _roleManager;
        private readonly MappedState<string, OrganizationCertificate> _organizationCertificateMap;
        private readonly MappedState<string, OrganizationUnit> _organizationUnitMap;
        private readonly MappedState<string, OrganizationDepartment> _organizationDepartmentMap;
        private readonly MappedState<string, string, bool> _organizationDepartmentPermissionMap;
        private readonly MappedState<string, StringList> _organizationDepartmentPermissionListMap;

        public OrganizationUnitManager(CSharpSmartContractContext context, RoleManager roleManager,
            MappedState<string, OrganizationCertificate> organizationCertificateMap,
            MappedState<string, OrganizationUnit> organizationUnitMap,
            MappedState<string, OrganizationDepartment> organizationDepartmentMap,
            MappedState<string, string, bool> organizationDepartmentPermissionMap,
            MappedState<string, StringList> organizationDepartmentPermissionListMap)
        {
            _context = context;
            _roleManager = roleManager;
            _organizationCertificateMap = organizationCertificateMap;
            _organizationUnitMap = organizationUnitMap;
            _organizationDepartmentMap = organizationDepartmentMap;
            _organizationDepartmentPermissionMap = organizationDepartmentPermissionMap;
            _organizationDepartmentPermissionListMap = organizationDepartmentPermissionListMap;
        }

        public void Initialize()
        {
            // 添加管理员机构
            AddOrganizationUnit(new OrganizationUnit
            {
                OrganizationName = DelegatorContractConstants.Admin,
                CreateTime = _context.CurrentBlockTime,
                OrganizationCreator = DelegatorContractConstants.System,
                RoleName = DelegatorContractConstants.Admin,
                Enabled = true,
            });

            // 添加默认机构
            AddOrganizationUnit(new OrganizationUnit
            {
                OrganizationName = DelegatorContractConstants.DefaultOrganizationName,
                CreateTime = _context.CurrentBlockTime,
                OrganizationCreator = DelegatorContractConstants.System,
                RoleName = DelegatorContractConstants.DefaultRoleName,
                Enabled = true
            });
        }

        public void AddOrganizationCertificate(OrganizationCertificate organizationCertificate)
        {
            if (_organizationCertificateMap[organizationCertificate.OrganizationName] != null)
            {
                throw new AssertionException($"机构 {organizationCertificate.OrganizationName} 已经提交过认证了");
            }

            organizationCertificate.CreateTime = _context.CurrentBlockTime;
            organizationCertificate.LatestEditTime = _context.CurrentBlockTime;
            _organizationCertificateMap[organizationCertificate.OrganizationName] = organizationCertificate;

            _context.Fire(new OrganizationCertificateCreated
            {
                FromId = organizationCertificate.Applier,
                OrganizationCertificate = organizationCertificate
            });
        }

        public void AddOrganizationUnit(OrganizationUnit organizationUnit)
        {
            organizationUnit.DepartmentList = new StringList();
            _roleManager.AddOrganizationUnit(organizationUnit);
            _organizationUnitMap[organizationUnit.OrganizationName] = organizationUnit;
            AddDefaultDepartmentsForOrganization(organizationUnit.OrganizationName);

            _context.Fire(new OrganizationUnitCreated
            {
                FromId = organizationUnit.OrganizationCreator,
                OrganizationUnit = organizationUnit
            });
        }

        public void AddOrganizationDepartment(OrganizationDepartment organizationDepartment)
        {
            var departmentKey = KeyHelper.GetOrganizationDepartmentKey(organizationDepartment.OrganizationName,
                organizationDepartment.DepartmentName);
            if (_organizationDepartmentMap[departmentKey] != null)
            {
                throw new AssertionException(
                    $"机构 {organizationDepartment.OrganizationName} 的部门 {organizationDepartment.DepartmentName} 已经存在了");
            }

            _organizationDepartmentMap[departmentKey] = new OrganizationDepartment
            {
                OrganizationName = organizationDepartment.OrganizationName,
                DepartmentName = organizationDepartment.DepartmentName,
                MemberList = new StringList()
            };
            _organizationUnitMap[organizationDepartment.OrganizationName].DepartmentList.Value
                .Add(organizationDepartment.DepartmentName);
            
            InheritPermissionListFromRole(organizationDepartment.OrganizationName, organizationDepartment.DepartmentName,
                new AdminPermissionSetStrategy());
        }

        public void AddDefaultDepartmentsForOrganization(string organizationName)
        {
            AddOrganizationDepartment(new OrganizationDepartment
            {
                OrganizationName = organizationName,
                DepartmentName = DelegatorContractConstants.Admin
            });

            AddOrganizationDepartment(new OrganizationDepartment
            {
                OrganizationName = organizationName,
                DepartmentName = DelegatorContractConstants.Member
            });
        }

        public void AddUser(User user)
        {
            AssertOrganizationUnitExists(user.OrganizationName);
            AssertOrganizationDepartmentExists(user.OrganizationName, user.OrganizationDepartmentName);
            var departmentKey =
                KeyHelper.GetOrganizationDepartmentKey(user.OrganizationName, user.OrganizationDepartmentName);
            _organizationDepartmentMap[departmentKey].MemberList.Value.Add(user.UserName);

            if (user.Enabled)
            {
                _organizationUnitMap[user.OrganizationName].UserCount++;
                _roleManager.AddUserCount(_organizationUnitMap[user.OrganizationName].RoleName);
            }
        }

        public void SubUserCount(string organizationName)
        {
            AssertOrganizationUnitExists(organizationName);

            _organizationUnitMap[organizationName].UserCount--;

            _roleManager.SubUserCount(_organizationUnitMap[organizationName].RoleName);
        }

        public OrganizationCertificate GetOrganizationCertificate(string organizationName)
        {
            AssertOrganizationCertificateExists(organizationName);
            return _organizationCertificateMap[organizationName];
        }

        public void AssertOrganizationCertificateExists(string organizationName)
        {
            if (_organizationCertificateMap[organizationName] == null)
            {
                throw new AssertionException($"{organizationName} 的机构认证未提交");
            }
        }

        public OrganizationUnit GetOrganizationUnit(string organizationName)
        {
            AssertOrganizationUnitExists(organizationName);
            return _organizationUnitMap[organizationName];
        }

        public void AssertOrganizationUnitExists(string organizationName)
        {
            if (_organizationUnitMap[organizationName] == null)
            {
                throw new AssertionException($"机构 {organizationName} 不存在");
            }
        }

        public void AssertOrganizationDepartmentExists(string organizationName, string departmentName)
        {
            if (_organizationDepartmentMap[KeyHelper.GetOrganizationDepartmentKey(organizationName, departmentName)] ==
                null)
            {
                throw new AssertionException($"机构 {organizationName} 不存在部门 {departmentName}");
            }
        }

        private void InheritPermissionListFromRole(string organizationName, string departmentName,
            IPermissionSetStrategy permissionSetStrategy)
        {
            AssertOrganizationUnitExists(organizationName);
            AssertOrganizationDepartmentExists(organizationName, departmentName);
            var rolePermissionList = _roleManager.GetRolePermissionList(GetOrganizationUnit(organizationName).RoleName);
            var departmentPermissionList =
                permissionSetStrategy.ExtractPermissionList(rolePermissionList);
            var departmentKey = KeyHelper.GetOrganizationDepartmentKey(organizationName, departmentName);
            foreach (var permissionId in departmentPermissionList)
            {
                _organizationDepartmentPermissionMap[departmentKey][permissionId] = true;
            }

            _organizationDepartmentPermissionListMap[departmentKey] = new StringList
            {
                Value = { departmentPermissionList }
            };
        }


    }
}