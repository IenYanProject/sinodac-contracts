using System.Linq;
using System.Threading.Tasks;
using AElf.ContractTestBase.ContractTestKit;
using AElf.Kernel;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Sinodac.Contracts.Delegator;
using Xunit;

namespace Sinodac.Contracts.DAC
{
    public class DACContractTests : DACContractTestBase
    {
        [Fact]
        public async Task Test()
        {
            var keyPair = SampleAccount.Accounts.First().KeyPair;
            var stub = GetDACContractStub(keyPair);

            // Use CallAsync or SendAsync method of this stub to test.
            // await stub.Hello.SendAsync(new Empty())

            // Or maybe you want to get its return value.
            // var output = (await stub.Hello.SendAsync(new Empty())).Output;

            // Or transaction result.
            // var transactionResult = (await stub.Hello.SendAsync(new Empty())).TransactionResult;
        }

        private async Task<DelegatorContractContainer.DelegatorContractStub> InitializeAsync()
        {
            var adminAccount = SampleAccount.Accounts.First();
            var delegatorContractStub = GetDelegatorContractStub(adminAccount.KeyPair);
            await delegatorContractStub.Initialize.SendAsync(new Delegator.InitializeInput
            {
                AdminAddress = adminAccount.Address
            });
            return delegatorContractStub;
        }

        [Fact]
        public async Task Permission_InitializeTest()
        {
            var delegatorContractStub = await InitializeAsync();

            {
                var organization = await delegatorContractStub.GetOrganizationUnit.CallAsync(
                    new StringValue
                    {
                        Value = "管理员"
                    });
                organization.AdminList.Value.Count.ShouldBe(1);
                organization.RoleName.ShouldBe("管理员");
                organization.UserCount.ShouldBe(1);
            }

            {
                var organization = await delegatorContractStub.GetOrganizationUnit.CallAsync(
                    new StringValue
                    {
                        Value = "默认机构"
                    });
                organization.AdminList.ShouldBeNull();
                organization.RoleName.ShouldBe("默认");
            }

            {
                var user = await delegatorContractStub.GetUser.CallAsync(new StringValue
                {
                    Value = "admin"
                });
                user.UserName.ShouldBe("admin");
                user.OrganizationName.ShouldBe("管理员");
                user.UserCreator.ShouldBe("系统");
            }
        }

        [Fact]
        public async Task Permission_CreateTest()
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
                    "Permission:User:Create"
                }
            });

            {
                var role = await delegatorContractStub.GetRole.CallAsync(new StringValue
                {
                    Value = "博物馆"
                });
                role.RoleCreator.ShouldBe("admin");
                role.Enabled.ShouldBeTrue();
                role.LatestEditTime.ShouldBeNull();
            }

            // 管理员创建一个账户alice
            await delegatorContractStub.CreateUser.SendAsync(new CreateUserInput
            {
                FromId = "admin",
                OrganizationName = "默认机构",
                UserName = "alice",
                Enable = true
            });

            // 账户alice申请成立Alice博物馆
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

            // 管理员admin通过认证
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
            organizationUnit.AdminList.Value.ShouldContain("alice");
        }
    }
}