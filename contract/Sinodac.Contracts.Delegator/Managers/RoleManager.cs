using System.Collections.Generic;
using System.Linq;
using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;
using Sinodac.Contracts.Delegator.Helpers;

namespace Sinodac.Contracts.Delegator.Managers
{
    public class RoleManager
    {
        private readonly CSharpSmartContractContext _context;
        private readonly MappedState<string, Role> _roleMap;
        private readonly MappedState<string, string, bool> _rolePermissionMap;
        private readonly MappedState<string, StringList> _rolePermissionListMap;
        private readonly MappedState<string, StringList> _roleOrganizationUnitListMap;

        public RoleManager(CSharpSmartContractContext context, MappedState<string, Role> roleMap,
            MappedState<string, string, bool> rolePermissionMap,
            MappedState<string, StringList> rolePermissionListMap,
            MappedState<string, StringList> roleOrganizationUnitListMap)
        {
            _context = context;
            _roleMap = roleMap;
            _rolePermissionMap = rolePermissionMap;
            _rolePermissionListMap = rolePermissionListMap;
            _roleOrganizationUnitListMap = roleOrganizationUnitListMap;
        }

        public void Initialize()
        {
            AddRole(new Role
            {
                RoleCreator = DelegatorContractConstants.System,
                RoleName = DelegatorContractConstants.Admin,
                Enabled = true,
                RoleDescription = "系统管理员"
            });
            InitialPermissionListToRole(DelegatorContractConstants.Admin, PermissionHelper.GetAllPermissionIdList());

            AddRole(new Role
            {
                RoleCreator = DelegatorContractConstants.System,
                RoleName = DelegatorContractConstants.DefaultRoleName,
                Enabled = true,
                RoleDescription = "不属于任何机构的角色"
            });
            InitialPermissionListToRole(DelegatorContractConstants.DefaultRoleName,
                PermissionHelper.GetDefaultPermissionIdList());
        }

        public void AddRole(Role role)
        {
            if (_roleMap[role.RoleName] != null)
            {
                throw new AssertionException($"角色 {role.RoleName} 已经存在了");
            }

            role.CreateTime = _context.CurrentBlockTime;
            role.LatestEditTime = _context.CurrentBlockTime;
            _roleMap[role.RoleName] = role;
            _roleOrganizationUnitListMap[role.RoleName] = new StringList();
            _context.Fire(new RoleCreated
            {
                FromId = role.RoleCreator,
                Role = role
            });
        }

        public void AddOrganizationUnit(OrganizationUnit organizationUnit)
        {
            IsRoleExists(organizationUnit.RoleName);
            _roleMap[organizationUnit.RoleName].OrganizationUnitCount++;
            _roleOrganizationUnitListMap[organizationUnit.RoleName].Value.Add(organizationUnit.OrganizationName);
        }

        public void AddUser(string roleName)
        {
            IsRoleExists(roleName);
            _roleMap[roleName].UserCount++;
        }

        public void InitialPermissionListToRole(string roleName, List<string> permissionList)
        {
            IsRoleExists(roleName);
            foreach (var permissionId in permissionList)
            {
                _rolePermissionMap[roleName][permissionId] = true;
            }

            _rolePermissionListMap[roleName] = new StringList
            {
                Value = { permissionList }
            };
        }

        public Role GetRole(string roleName)
        {
            IsRoleExists(roleName);
            return _roleMap[roleName];
        }

        public void IsRoleExists(string roleName)
        {
            if (_roleMap[roleName] == null)
            {
                throw new AssertionException($"角色 {roleName} 不存在");
            }
        }

        public List<string> GetRolePermissionList(string roleName)
        {
            return _rolePermissionListMap[roleName].Value.ToList();
        }
    }
}