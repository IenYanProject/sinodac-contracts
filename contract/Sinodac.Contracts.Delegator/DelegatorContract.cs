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

            const string admin = "管理员";
            const string defaultOrganizationName = "默认机构";
            const string defaultRoleName = "默认角色";
            const string system = "系统";

            State.RoleMap[admin] = new Role
            {
                RoleName = admin,
                RoleCreator = system,
                CreateTime = Context.CurrentBlockTime,
                Enabled = true,
                RoleDescription = $"{admin} can do anything.",
            };
            State.OrganizationUnitMap[admin] = new OrganizationUnit
            {
                OrganizationName = admin,
                CreateTime = Context.CurrentBlockTime,
                Enabled = true,
                OrganizationCreator = system,
                RoleName = admin,
            };
            State.RoleOrganizationUnitListMap[admin] = new StringList
            {
                Value = { admin }
            };

            State.RoleMap[defaultRoleName] = new Role
            {
                RoleName = defaultRoleName,
                RoleCreator = system,
                CreateTime = Context.CurrentBlockTime,
                Enabled = true,
                RoleDescription = "缺省设置",
            };
            State.OrganizationUnitMap[defaultOrganizationName] = new OrganizationUnit
            {
                OrganizationName = defaultOrganizationName,
                CreateTime = Context.CurrentBlockTime,
                Enabled = true,
                OrganizationCreator = system,
                RoleName = defaultRoleName,
            };
            State.RoleOrganizationUnitListMap[defaultRoleName] = new StringList
            {
                Value = { defaultOrganizationName }
            };

            foreach (var actionId in GetAllActionIds())
            {
                State.RolePermissionMap[admin][actionId] = true;
            }

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