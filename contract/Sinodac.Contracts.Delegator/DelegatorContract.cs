using AElf;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract : DelegatorContractContainer.DelegatorContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            Assert(State.Admin.Value == null, "合约已经完成过初始化了");
            State.Admin.Value = input.AdminAddress ?? Context.Sender;
            var roleManager = GetRoleManager();
            roleManager.Initialize();
            var organizationUnitManager = GetOrganizationUnitManager(roleManager);
            organizationUnitManager.Initialize();
            var userManager = GetUserManager(roleManager, organizationUnitManager);
            userManager.Initialize();

            State.DACContract.Value = input.DacContractAddress;
            State.DACMarketContract.Value = input.DacMarketContractAddress;
            return new Empty();
        }

        public override Empty RegisterSenders(RegisterSendersInput input)
        {
            if (!input.IsRemove)
            {
                foreach (var address in input.AddressList.Value)
                {
                    State.IsPermittedAddressMap[Context.Sender][input.ScopeId][address] = true;
                }
            }
            else
            {
                foreach (var address in input.AddressList.Value)
                {
                    State.IsPermittedAddressMap[Context.Sender][input.ScopeId].Remove(address);
                }
            }

            return new Empty();
        }

        public override Empty RegisterMethods(RegisterMethodsInput input)
        {
            if (!input.IsRemove)
            {
                foreach (var methodName in input.MethodNameList.Value)
                {
                    State.IsPermittedMethodNameMap[Context.Sender][input.ScopeId][methodName] = true;
                }
            }
            else
            {
                foreach (var methodName in input.MethodNameList.Value)
                {
                    State.IsPermittedMethodNameMap[Context.Sender][input.ScopeId].Remove(methodName);
                }
            }

            return new Empty();
        }

        public override Empty Forward(ForwardInput input)
        {
            var user = State.UserMap[input.FromId];
            if (user == null)
            {
                throw new AssertionException($"用户 {input.FromId} 不存在");
            }
            var organization = State.OrganizationUnitMap[user.OrganizationName];
            Assert(State.RolePermissionMap[organization.RoleName][input.ScopeId],
                $"用户所属的角色 {organization.RoleName} 没有 {input.ScopeId} 权限");
            Assert(State.IsPermittedAddressMap[input.ToAddress][input.ScopeId][Context.Sender],
                "交易发起人无权限");
            Assert(State.IsPermittedMethodNameMap[input.ToAddress][input.ScopeId][input.MethodName],
                $"交易发起人没有调用 {input.MethodName} 的权限");
            var fromHash = HashHelper.ComputeFrom($"{input.ScopeId}-{input.FromId}");
            Context.SendVirtualInline(fromHash, input.ToAddress, input.MethodName,
                input.Parameter);
            State.ForwardRecordMap[Context.TransactionId] = new ForwardRecord
            {
                VirtualFromAddress = Context.ConvertVirtualAddressToContractAddress(fromHash),
                ToAddress = input.ToAddress,
                FromId = input.FromId,
                MethodName = input.MethodName,
                Parameter = input.Parameter,
                ScopeId = input.ScopeId
            };
            State.TemporaryTxIdMap[Context.TransactionId] = true;
            return new Empty();
        }

        public override Empty ForwardCheck(Hash input)
        {
            Assert(State.TemporaryTxIdMap[input], "Forward check failed.");
            State.TemporaryTxIdMap.Remove(input);
            return new Empty();
        }

        public override ForwardRecord GetForwardRecord(Hash input)
        {
            return State.ForwardRecordMap[input];
        }

        public override BoolValue IsPermittedAddress(IsPermittedAddressInput input)
        {
            return new BoolValue { Value = State.IsPermittedAddressMap[input.ToAddress][input.ScopeId][input.Address] };
        }

        public override BoolValue IsPermittedMethod(IsPermittedMethodInput input)
        {
            return new BoolValue
            {
                Value = State.IsPermittedMethodNameMap[input.ToAddress][input.ScopeId][input.MethodName]
            };
        }
    }
}