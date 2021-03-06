using System.Linq;
using System.Threading.Tasks;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Kernel;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Sinodac.Contracts.Delegator;
using Sinodac.Contracts.Delegator.Helpers;
using Xunit;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContractTests : DACContractTestBase
    {
        /// <summary>
        /// 完成初始化后，预期：
        /// - 有两个角色：管理员 & 默认
        /// - 管理员角色拥有所有权限；默认角色拥有Profile相关权限
        /// - 有两个机构：管理员 & 默认机构
        /// - 每个机构都有两个部门：管理员 & 员工
        /// - 管理员部门拥有机构对应角色的所有权限；员工部门拥有比管理员部门少了User相关权限
        /// - 有一个用户：admin，是管理员机构的管理员
        /// </summary>
        [Fact(DisplayName = "【Permission】检查合约初始化结果", Skip = "")]
        public async Task Permission_InitializeTest()
        {
            var delegatorContractStub = await InitializeAsync();

            {
                var role = await delegatorContractStub.GetRole.CallAsync(new StringValue
                {
                    Value = "管理员"
                });
                role.RoleName.ShouldBe("管理员");
                role.RoleCreator.ShouldBe("系统");
                role.OrganizationUnitCount.ShouldBe(1);
                role.UserCount.ShouldBe(1);

                var rolePermissionList = await delegatorContractStub.GetRolePermissionList.CallAsync(new StringValue
                {
                    Value = "管理员"
                });
                rolePermissionList.Value.Count.ShouldBeGreaterThan(10);
            }

            {
                var role = await delegatorContractStub.GetRole.CallAsync(new StringValue
                {
                    Value = "默认"
                });
                role.RoleName.ShouldBe("默认");
                role.RoleCreator.ShouldBe("系统");
                role.OrganizationUnitCount.ShouldBe(1);

                var rolePermissionList = await delegatorContractStub.GetRolePermissionList.CallAsync(new StringValue
                {
                    Value = "默认"
                });
                rolePermissionList.Value.Count.ShouldBe(4);
            }

            {
                var organization = await delegatorContractStub.GetOrganizationUnit.CallAsync(
                    new StringValue
                    {
                        Value = "管理员"
                    });
                organization.OrganizationName.ShouldBe("管理员");
                organization.DepartmentList.Value.Count.ShouldBe(2);
                organization.RoleName.ShouldBe("管理员");
                organization.UserCount.ShouldBe(1);
            }

            {
                var department = await delegatorContractStub.GetOrganizationDepartment.CallAsync(new StringValue
                {
                    Value = KeyHelper.GetOrganizationDepartmentKey("管理员", "管理员")
                });
                department.OrganizationName.ShouldBe("管理员");
                department.DepartmentName.ShouldBe("管理员");
                department.MemberList.Value.Count.ShouldBe(1);
                department.MemberList.Value.First().ShouldBe("admin");
                var departmentIgnoredPermissionList =
                    await delegatorContractStub.GetOrganizationDepartmentIgnoredPermissionList.CallAsync(
                        new GetOrganizationDepartmentIgnoredPermissionListInput
                        {
                            OrganizationName = "管理员",
                            DepartmentName = "管理员"
                        });
                departmentIgnoredPermissionList.Value.Count.ShouldBe(0);
            }

            {
                var department = await delegatorContractStub.GetOrganizationDepartment.CallAsync(new StringValue
                {
                    Value = KeyHelper.GetOrganizationDepartmentKey("管理员", "员工")
                });
                department.OrganizationName.ShouldBe("管理员");
                department.DepartmentName.ShouldBe("员工");
                department.MemberList.Value.Count.ShouldBe(0);
                var departmentIgnoredPermissionList =
                    await delegatorContractStub.GetOrganizationDepartmentIgnoredPermissionList.CallAsync(
                        new GetOrganizationDepartmentIgnoredPermissionListInput
                        {
                            OrganizationName = "管理员",
                            DepartmentName = "员工"
                        });
                departmentIgnoredPermissionList.Value.Count.ShouldBe(4);
            }

            {
                var organization = await delegatorContractStub.GetOrganizationUnit.CallAsync(
                    new StringValue
                    {
                        Value = "默认机构"
                    });
                organization.OrganizationName.ShouldBe("默认机构");
                organization.DepartmentList.Value.Count.ShouldBe(2);
                organization.RoleName.ShouldBe("默认");
                organization.UserCount.ShouldBe(0);
            }

            {
                var department = await delegatorContractStub.GetOrganizationDepartment.CallAsync(new StringValue
                {
                    Value = KeyHelper.GetOrganizationDepartmentKey("默认机构", "管理员")
                });
                department.OrganizationName.ShouldBe("默认机构");
                department.DepartmentName.ShouldBe("管理员");
                var departmentIgnoredPermissionList =
                    await delegatorContractStub.GetOrganizationDepartmentIgnoredPermissionList.CallAsync(
                        new GetOrganizationDepartmentIgnoredPermissionListInput
                        {
                            OrganizationName = "默认机构",
                            DepartmentName = "管理员"
                        });
                departmentIgnoredPermissionList.Value.Count.ShouldBe(0);
            }

            {
                var department = await delegatorContractStub.GetOrganizationDepartment.CallAsync(new StringValue
                {
                    Value = KeyHelper.GetOrganizationDepartmentKey("默认机构", "员工")
                });
                department.OrganizationName.ShouldBe("默认机构");
                department.DepartmentName.ShouldBe("员工");
                var departmentIgnoredPermissionList =
                    await delegatorContractStub.GetOrganizationDepartmentIgnoredPermissionList.CallAsync(
                        new GetOrganizationDepartmentIgnoredPermissionListInput
                        {
                            OrganizationName = "默认机构",
                            DepartmentName = "员工"
                        });
                departmentIgnoredPermissionList.Value.Count.ShouldBe(4);
            }

            {
                var user = await delegatorContractStub.GetUser.CallAsync(new StringValue
                {
                    Value = "admin"
                });
                user.UserName.ShouldBe("admin");
                user.OrganizationName.ShouldBe("管理员");
                user.OrganizationDepartmentName.ShouldBe("管理员");
                user.UserCreator.ShouldBe("系统");
            }
        }

        [Fact(DisplayName = "【Permission】创建角色：博物馆", Skip = "")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> Permission_CreateRoleTest()
        {
            var delegatorContractStub = await InitializeAsync();

            await delegatorContractStub.CreateRole.SendAsync(new CreateRoleInput
            {
                RoleName = "博物馆",
                Enable = true,
                FromId = "admin",
                RoleDescription = "博物馆角色",
                PermissionList =
                {
                    PermissionHelper.GetMuseumPermissionIdList()
                }
            });

            {
                var role = await delegatorContractStub.GetRole.CallAsync(new StringValue
                {
                    Value = "博物馆"
                });
                role.RoleCreator.ShouldBe("admin");
                role.Enabled.ShouldBeTrue();
            }

            return delegatorContractStub;
        }

        [Fact(DisplayName = "【Permission】创建用户：alice", Skip = "")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> Permission_CreateUserTest()
        {
            var delegatorContractStub = await Permission_CreateRoleTest();

            await delegatorContractStub.CreateUser.SendAsync(new CreateUserInput
            {
                FromId = "admin",
                UserName = "alice",
                Enable = true
            });

            var user = await delegatorContractStub.GetUser.CallAsync(new StringValue
            {
                Value = "alice"
            });
            user.OrganizationName.ShouldBe("默认机构");
            user.OrganizationDepartmentName.ShouldBe("员工");

            // 默认角色和默认机构的UserCount该变成1了
            var role = await delegatorContractStub.GetRole.CallAsync(new StringValue
            {
                Value = "默认"
            });
            role.UserCount.ShouldBe(1);
            var organizationUnit = await delegatorContractStub.GetOrganizationUnit.CallAsync(new StringValue
            {
                Value = "默认机构"
            });
            role.UserCount.ShouldBe(1);

            return delegatorContractStub;
        }

        [Fact(DisplayName = "【Permission】用户alice提交机构认证", Skip = "")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> Permission_OrganizationCertificateTest()
        {
            var delegatorContractStub = await Permission_CreateUserTest();

            await delegatorContractStub.CreateOrganizationCertificate.SendAsync(new CreateOrganizationCertificateInput
            {
                FromId = "alice",
                OrganizationName = "Alice博物馆",
                OrganizationDescription = "里面都是Alice的藏品",
                OrganizationEmail = "alice@aelf.io",
                OrganizationLevel = 1,
                OrganizationLocation = "地球",
                OrganizationType = 1,
                OrganizationArtificialPerson = "艾丽丝",
                OrganizationCreditCode = "I_KNOW_NOTHING",
                OrganizationEstablishedTime = TimestampHelper.GetUtcNow(),
                RegistrationAuthority = "时空管理局",
                OrganizationPhoneNumber = "9527",
                PhotoIds = { "1", "2" }
            });

            var organizationCertificate = await delegatorContractStub.GetOrganizationCertificate.CallAsync(
                new StringValue
                {
                    Value = "Alice博物馆"
                });
            organizationCertificate.Applier.ShouldBe("alice");

            return delegatorContractStub;
        }

        [Fact(DisplayName = "【Permission】admin通过alice的机构认证", Skip = "")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> Permission_CreateOrganizationUnitTest()
        {
            var delegatorContractStub = await Permission_OrganizationCertificateTest();

            await delegatorContractStub.CreateOrganizationUnit.SendAsync(new CreateOrganizationUnitInput
            {
                FromId = "admin",
                OrganizationName = "Alice博物馆",
                Enable = true,
                IsApprove = true,
                RoleName = "博物馆"
            });

            var organizationUnit = await delegatorContractStub.GetOrganizationUnit.CallAsync(new StringValue
            {
                Value = "Alice博物馆"
            });
            organizationUnit.IsApproved.ShouldBeTrue();
            organizationUnit.DepartmentList.Value.Count.ShouldBe(2);
            organizationUnit.UserCount.ShouldBe(1);

            return delegatorContractStub;
        }

        [Fact(DisplayName = "【Permission】alice创建机构管理员bob，bob创建机构员工", Skip = "")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> Permission_CreateMemberTest()
        {
            var delegatorContractStub = await Permission_CreateOrganizationUnitTest();

            // alice创建用户：机构管理员bob
            await delegatorContractStub.CreateUser.SendAsync(new CreateUserInput
            {
                FromId = "alice",
                OrganizationName = "Alice博物馆",
                UserName = "bob",
                Enable = true,
                DepartmentName = "管理员"
            });

            {
                var user = await delegatorContractStub.GetUser.CallAsync(new StringValue
                {
                    Value = "bob"
                });
                user.Enabled.ShouldBeTrue();
                user.OrganizationDepartmentName.ShouldBe("管理员");
            }

            // bob创建用户：机构员工clare
            await delegatorContractStub.CreateUser.SendAsync(new CreateUserInput
            {
                OrganizationName = "Alice博物馆",
                FromId = "bob",
                UserName = "clare",
                Enable = true
            });

            {
                var user = await delegatorContractStub.GetUser.CallAsync(new StringValue
                {
                    Value = "clare"
                });
                user.Enabled.ShouldBeTrue();
                user.OrganizationDepartmentName.ShouldBe("员工");
            }
            return delegatorContractStub;
        }

        [Fact(DisplayName = "【Permission】bob创建默认机构用户", Skip = "")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> Permission_CreateDefaultUserTest()
        {
            var delegatorContractStub = await Permission_CreateMemberTest();

            // bob创建用户：非本机构员工mao
            await delegatorContractStub.CreateUser.SendAsync(new CreateUserInput
            {
                FromId = "bob",
                UserName = "mao",
                Enable = true
            });

            // mao会进入默认机构
            {
                var dart = await delegatorContractStub.GetUser.CallAsync(new StringValue
                {
                    Value = "mao"
                });
                dart.OrganizationName.ShouldBe("默认机构");
                dart.OrganizationDepartmentName.ShouldBe("员工");
            }

            return delegatorContractStub;
        }

        [Fact(DisplayName = "【Permission】mao提交个人艺术家认证", Skip = "")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> Permission_IndependentCertificateTest()
        {
            var delegatorContractStub = await Permission_CreateDefaultUserTest();
            // mao提交个人艺术家认证
            await delegatorContractStub.CreateIndependentCertificate.SendAsync(new CreateIndependentCertificateInput
            {
                FromId = "mao",
                Id = "PRETENDING_TO_BE_AN_ID_NUMBER",
                Description = "艺术带师",
                Email = "PRETENDING_TO_BE_AN_EMAIL_ADDRESS",
                Location = "火星",
                Name = "毛线",
                PhoneNumber = "13188888888",
                PhotoIds = { "run", "tu", "ci", "cha" }
            });

            {
                var dartCertificate = await delegatorContractStub.GetIndependentCertificate.CallAsync(
                    new StringValue
                    {
                        Value = "mao"
                    });
                dartCertificate.Name.ShouldBe("毛线");
                dartCertificate.Description.ShouldBe("艺术带师");
            }
            return delegatorContractStub;
        }


        [Fact(DisplayName = "【Permission】admin通过mao的个人艺术家认证", Skip = "")]
        public async Task Permission_CreateIndependentArtist()
        {
            var delegatorContractStub = await Permission_IndependentCertificateTest();

            await delegatorContractStub.CreateIndependentArtist.SendAsync(new CreateIndependentArtistInput
            {
                ArtistUserName = "mao",
                FromId = "admin",
                Enable = true,
                IsApprove = true
            });
        }

        private async Task<DelegatorContractContainer.DelegatorContractStub> InitializeAsync()
        {
            var adminAccount = SampleAccount.Accounts.First();
            var delegatorContractStub = GetDelegatorContractStub(adminAccount.KeyPair);
            await delegatorContractStub.Initialize.SendAsync(new Delegator.InitializeInput
            {
                AdminAddress = adminAccount.Address,
                DacContractAddress = DAppContractAddress,
                DacMarketContractAddress = DACMarketContractAddress
            });

            return delegatorContractStub;
        }
    }
}