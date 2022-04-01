using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        /// <summary>
        /// 提交机构认证
        /// 需要权限：Profile:Certificate:OrganizationUnit（默认角色）
        /// 创建和存储OrganizationCertificate实例，key为机构名称（organizationName）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Empty CreateOrganizationCertificate(CreateOrganizationCertificateInput input)
        {
            AssertPermission(input.FromId, false, Profile.CertificateOrganizationUnit);
            Assert(
                State.OrganizationCertificateMap[input.OrganizationName] == null,
                $"机构 {input.OrganizationName} 已经提交过认证了");
            var organizationCertificate = new OrganizationCertificate
            {
                CreateTime = Context.CurrentBlockTime,
                OrganizationDescription = input.OrganizationDescription,
                OrganizationEmail = input.OrganizationEmail,
                OrganizationLevel = input.OrganizationLevel,
                OrganizationLocation = input.OrganizationLocation,
                OrganizationName = input.OrganizationName,
                OrganizationType = input.OrganizationType,
                OrganizationArtificialPerson = input.OrganizationArtificialPerson,
                OrganizationCreditCode = input.OrganizationCreditCode,
                OrganizationEstablishedTime = input.OrganizationEstablishedTime,
                OrganizationPhoneNumber = input.OrganizationPhoneNumber,
                RegistrationAuthority = input.RegistrationAuthority,
                PhotoIds = { input.PhotoIds },
                Applier = input.FromId
            };
            State.OrganizationCertificateMap[input.OrganizationName] = organizationCertificate;
            Context.Fire(new OrganizationCertificateCreated
            {
                FromId = input.FromId,
                OrganizationCertificate = organizationCertificate
            });
            return new Empty();
        }

        /// <summary>
        /// 机构认证被驳回后，申请人可修改认证信息
        /// 需要权限：Profile.CertificateOrganizationUnit，但需要是这个组织的管理员
        /// 可以用来修改各种机构认证的信息，除了创建时间
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="AssertionException"></exception>
        public override Empty UpdateOrganizationCertificate(UpdateOrganizationCertificateInput input)
        {
            AssertPermission(input.FromId, true, Profile.CertificateOrganizationUnit);
            var organizationCertificate = State.OrganizationCertificateMap[input.OrganizationName];
            if (organizationCertificate == null)
            {
                throw new AssertionException($"机构 {input.OrganizationName} 未曾提交过认证");
            }

            Assert(organizationCertificate.IsRejected, "被驳回后才可以更新认证信息");

            State.OrganizationCertificateMap[input.OrganizationName] = new OrganizationCertificate
            {
                CreateTime = organizationCertificate.CreateTime,
                OrganizationDescription = input.OrganizationDescription,
                OrganizationEmail = input.OrganizationEmail,
                OrganizationLevel = input.OrganizationLevel,
                OrganizationLocation = input.OrganizationLocation,
                OrganizationName = input.OrganizationName,
                OrganizationType = input.OrganizationType,
                OrganizationArtificialPerson = input.OrganizationArtificialPerson,
                OrganizationCreditCode = input.OrganizationCreditCode,
                OrganizationEstablishedTime = input.OrganizationEstablishedTime,
                OrganizationPhoneNumber = input.OrganizationPhoneNumber,
                RegistrationAuthority = input.RegistrationAuthority,
                PhotoIds = { input.PhotoIds },
                Applier = input.FromId,
                LatestEditTime = Context.CurrentBlockTime
            };
            return new Empty();
        }

        /// <summary>
        /// 通过或驳回机构认证
        /// 需要权限：Permission.OrganizationUnit.Create（管理员角色）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="AssertionException"></exception>
        public override Empty CreateOrganizationUnit(CreateOrganizationUnitInput input)
        {
            AssertPermission(input.FromId, false, Permission.OrganizationUnit.Create);

            var organizationCertificate = State.OrganizationCertificateMap[input.OrganizationName];
            if (organizationCertificate == null)
            {
                throw new AssertionException($"机构 {input.OrganizationName} 未曾提交过认证");
            }

            if (!input.IsApprove)
            {
                State.OrganizationCertificateMap[input.OrganizationName].IsRejected = true;
                Context.Fire(new OrganizationCertificateRejected
                {
                    FromId = input.FromId,
                    OrganizationName = input.OrganizationName
                });
                return new Empty();
            }

            var role = State.RoleMap[input.RoleName];
            if (role == null)
            {
                throw new AssertionException($"角色 {input.RoleName} 不存在");
            }

            Assert(role.Enabled, $"角色 {input.RoleName} 当前被禁用");

            State.RoleMap[input.RoleName].OrganizationUnitCount = role.OrganizationUnitCount.Add(1);

            var organizationUnit = new OrganizationUnit
            {
                OrganizationName = input.OrganizationName,
                OrganizationCreator = organizationCertificate.Applier,
                Enabled = input.Enable,
                RoleName = input.RoleName,
                CreateTime = Context.CurrentBlockTime,
                AdminList = new StringList
                {
                    Value = { organizationCertificate.Applier }
                },
                GroupList = new StringList
                {
                    Value = { Admin, Member }
                }
            };
            State.OrganizationUnitMap[input.OrganizationName] = organizationUnit;
            
            // Transfer Creator's profile to the new organization unit.
            var previewsOrganizationName = State.UserMap[organizationCertificate.Applier].OrganizationName;
            var previewsRoleName = State.OrganizationUnitMap[previewsOrganizationName].RoleName;
            State.UserMap[organizationCertificate.Applier].OrganizationName = input.OrganizationName;
            State.OrganizationUnitMap[previewsOrganizationName].UserCount =
                State.OrganizationUnitMap[previewsOrganizationName].UserCount.Sub(1);
            State.RoleMap[previewsRoleName].UserCount =
                State.RoleMap[previewsRoleName].UserCount.Sub(1);

            var adminPermissionList = State.RoleActionIdListMap[input.RoleName];
            State.GroupActionIdListMap[GetOrganizationAdminKey(input.OrganizationName)] = adminPermissionList;
            var memberPermissionList = adminPermissionList.Clone();
            if (memberPermissionList.Value.Contains(Permission.User.Create))
            {
                memberPermissionList.Value.Remove(Permission.User.Create);
            }
            State.GroupActionIdListMap[GetOrganizationMemberKey(input.OrganizationName)] = memberPermissionList;

            Context.Fire(new OrganizationUnitCreated
            {
                FromId = input.FromId,
                OrganizationUnit = organizationUnit,
                AdminPermissionList = adminPermissionList,
                MemberPermissionList = memberPermissionList
            });
            return new Empty();
        }

        public override Empty UpdateOrganizationUnit(UpdateOrganizationUnitInput input)
        {
            AssertPermission(input.FromId, true,
                Permission.OrganizationUnit.Update);

            var oldOrganizationUnit = State.OrganizationUnitMap[input.OrganizationName].Clone();

            var organizationUnit = new OrganizationUnit
            {
                OrganizationName = input.OrganizationName,
                RoleName = input.RoleName,
                // Stay old values.
                OrganizationCreator = oldOrganizationUnit.OrganizationCreator,
                Enabled = oldOrganizationUnit.Enabled,
                CreateTime = oldOrganizationUnit.CreateTime,
                UserCount = oldOrganizationUnit.UserCount
            };
            State.OrganizationUnitMap[input.OrganizationName] = organizationUnit;
            Context.Fire(new OrganizationUnitUpdated
            {
                FromId = input.FromId,
                OrganizationUnit = organizationUnit
            });
            return new Empty();
        }

        public override Empty DisableOrganizationUnit(DisableOrganizationUnitInput input)
        {
            AssertPermission(input.FromId, true, Permission.OrganizationUnit.Disable);

            var organizationUnit = State.OrganizationUnitMap[input.OrganizationName];

            if (input.Enable)
            {
                State.OrganizationUnitMap[input.OrganizationName].Enabled = true;
                Assert(State.RoleMap[organizationUnit.RoleName].Enabled,
                    $"机构 {input.OrganizationName} 所属角色 {organizationUnit.RoleName} 当前为禁用状态");
                State.RoleMap[organizationUnit.RoleName].OrganizationUnitCount =
                    State.RoleMap[organizationUnit.RoleName].OrganizationUnitCount.Add(1);
                Context.Fire(new OrganizationUnitEnabled
                {
                    FromId = input.FromId,
                    OrganizationName = input.OrganizationName
                });
                return new Empty();
            }

            var userList = State.OrganizationUnitUserListMap[input.OrganizationName];
            foreach (var user in userList.Value)
            {
                Assert(!State.UserMap[user].Enabled, $"当前角色下存在【启用】状态用户 {user} ，请先禁用该用户后再禁用当前角色");
            }

            State.OrganizationUnitMap[input.OrganizationName].Enabled = false;

            State.RoleMap[organizationUnit.RoleName].OrganizationUnitCount =
                State.RoleMap[organizationUnit.RoleName].OrganizationUnitCount.Sub(1);

            Context.Fire(new OrganizationUnitDisabled
            {
                FromId = input.FromId,
                OrganizationName = input.OrganizationName
            });
            return new Empty();
        }
        
        public override OrganizationUnitList GetOrganizationUnitList(GetOrganizationUnitListInput input)
        {
            return base.GetOrganizationUnitList(input);
        }
    }
}