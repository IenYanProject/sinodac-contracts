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
            var organizationUnitAdmin = new OrganizationUnit
            {
                OrganizationName = DelegatorContractConstants.Admin,
                CreateTime = _context.CurrentBlockTime,
                OrganizationCreator = DelegatorContractConstants.System,
                RoleName = DelegatorContractConstants.Admin,
                Enabled = true,
            };
            AddOrganizationUnit(organizationUnitAdmin);
            _roleManager.AddOrganizationUnit(organizationUnitAdmin);

            // 为管理员机构添加两个默认部门
            AddDefaultDepartmentsForOrganization(DelegatorContractConstants.Admin);

            // 添加默认机构
            var organizationUnitDefault = new OrganizationUnit
            {
                OrganizationName = DelegatorContractConstants.DefaultOrganizationName,
                CreateTime = _context.CurrentBlockTime,
                OrganizationCreator = DelegatorContractConstants.System,
                RoleName = DelegatorContractConstants.DefaultRoleName,
                Enabled = true
            };
            _roleManager.AddOrganizationUnit(organizationUnitDefault);
            AddOrganizationUnit(organizationUnitDefault);

            // 为默认机构添加两个默认部门
            AddDefaultDepartmentsForOrganization(DelegatorContractConstants.DefaultOrganizationName);
        }

        public void AddOrganizationCertificate()
        {

        }

        public void AddOrganizationUnit(OrganizationUnit organizationUnit)
        {
            organizationUnit.DepartmentList = new StringList();
            _roleManager.AddOrganizationUnit(organizationUnit);
            _organizationUnitMap[organizationUnit.OrganizationName] = organizationUnit;

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
                DepartmentName = organizationDepartment.DepartmentName
            };
            _organizationUnitMap[organizationDepartment.OrganizationName].DepartmentList.Value
                .Add(organizationDepartment.DepartmentName);
        }

        public void AddDefaultDepartmentsForOrganization(string organizationName)
        {
            AddOrganizationDepartment(new OrganizationDepartment
            {
                OrganizationName = organizationName,
                DepartmentName = DelegatorContractConstants.Admin
            });
            InheritPermissionListFromRole(organizationName, DelegatorContractConstants.Admin,
                new AdminPermissionSetStrategy());
            AddOrganizationDepartment(new OrganizationDepartment
            {
                OrganizationName = organizationName,
                DepartmentName = DelegatorContractConstants.Member
            });
            InheritPermissionListFromRole(organizationName, DelegatorContractConstants.Admin,
                new MemberPermissionStrategy());
        }

        public void AddUser(User user)
        {
            IsOrganizationUnitExists(user.OrganizationName);
            IsOrganizationDepartmentExists(user.OrganizationName, user.OrganizationDepartmentName);
            _organizationUnitMap[user.OrganizationName].UserCount++;
            _roleManager.AddUser(_organizationUnitMap[user.OrganizationName].RoleName);
        }

        public OrganizationCertificate GetOrganizationCertificate(string organizationName)
        {
            IsOrganizationCertificateExists(organizationName);
            return _organizationCertificateMap[organizationName];
        }

        public void IsOrganizationCertificateExists(string organizationName)
        {
            if (_organizationCertificateMap[organizationName] == null)
            {
                throw new AssertionException($"{organizationName} 的机构认证不存在");
            }
        }

        public OrganizationUnit GetOrganizationUnit(string organizationName)
        {
            IsOrganizationUnitExists(organizationName);
            return _organizationUnitMap[organizationName];
        }

        public void IsOrganizationUnitExists(string organizationName)
        {
            if (_organizationUnitMap[organizationName] == null)
            {
                throw new AssertionException($"{organizationName} 的机构认证不存在");
            }
        }

        public void IsOrganizationDepartmentExists(string organizationName, string departmentName)
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
            IsOrganizationUnitExists(organizationName);
            IsOrganizationDepartmentExists(organizationName, departmentName);
            var permissionList =
                permissionSetStrategy.ExtractPermissionList(
                    _roleManager.GetRolePermissionList(GetOrganizationUnit(organizationName).RoleName));
            var departmentKey = KeyHelper.GetOrganizationDepartmentKey(organizationName, departmentName);
            foreach (var permissionId in permissionList)
            {
                _organizationDepartmentPermissionMap[departmentKey][permissionId] = true;
            }

            _organizationDepartmentPermissionListMap[departmentKey] = new StringList
            {
                Value = { permissionList }
            };
        }
    }
}