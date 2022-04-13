using System.Collections.Generic;
using System.Linq;
using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;
using Google.Protobuf.Collections;
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
            InitialRolePermissionList(DelegatorContractConstants.Admin, PermissionHelper.GetAllPermissionIdList());

            AddRole(new Role
            {
                RoleCreator = DelegatorContractConstants.System,
                RoleName = DelegatorContractConstants.DefaultRoleName,
                Enabled = true,
                RoleDescription = "不属于任何机构的角色"
            });
            InitialRolePermissionList(DelegatorContractConstants.DefaultRoleName,
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

        public void UpdateRole(Role role)
        {
            _roleMap[role.RoleName] = role;
            _context.Fire(new RoleUpdated
            {
                FromId = role.LatestEditId,
                Role = role
            });
        }

        public void AddOrganizationUnit(OrganizationUnit organizationUnit)
        {
            AssertRoleExists(organizationUnit.RoleName);
            _roleMap[organizationUnit.RoleName].OrganizationUnitCount++;
            _roleOrganizationUnitListMap[organizationUnit.RoleName].Value.Add(organizationUnit.OrganizationName);
        }

        public void AddUser(User user)
        {
            var roleName = user.RoleName;
            AssertRoleExists(roleName);
            _roleMap[roleName].UserCount++;
        }

        public void SubUserCount(string roleName)
        {
            AssertRoleExists(roleName);
            _roleMap[roleName].UserCount--;
        }

        public void InitialRolePermissionList(string roleName, List<string> permissionList)
        {
            AssertRoleExists(roleName);
            foreach (var permissionId in permissionList)
            {
                _rolePermissionMap[roleName][permissionId] = true;
            }

            _rolePermissionListMap[roleName] = new StringList
            {
                Value = { permissionList }
            };
        }

        public void UpdateRolePermissionList(string roleName, RepeatedField<string> enablePermissionList,
            RepeatedField<string> disablePermissionList)
        {
            var previewsActionIdList = _rolePermissionListMap[roleName];
            foreach (var actionId in enablePermissionList)
            {
                _rolePermissionMap[roleName][actionId] = true;
                if (!previewsActionIdList.Value.Contains(actionId))
                {
                    previewsActionIdList.Value.Add(actionId);
                }
            }

            foreach (var actionId in disablePermissionList)
            {
                _rolePermissionMap[roleName].Remove(actionId);
                if (previewsActionIdList.Value.Contains(actionId))
                {
                    previewsActionIdList.Value.Remove(actionId);
                }
            }

            _rolePermissionListMap[roleName] = previewsActionIdList;
        }

        public void EnableRole(Role role)
        {
            UpdateRole(role);
            _context.Fire(new RoleEnabled
            {
                FromId = role.LatestEditId,
                RoleName = role.RoleName
            });
        }

        public void DisableRole(Role role)
        {
            UpdateRole(role);
            _context.Fire(new RoleDisabled
            {
                FromId = role.LatestEditId,
                RoleName = role.RoleName
            });
        }

        public Role GetRole(string roleName)
        {
            AssertRoleExists(roleName);
            return _roleMap[roleName];
        }

        public void AssertRoleExists(string roleName)
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