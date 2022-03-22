using AElf;
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

            const string admin = "Admin";
            const string defaultOrganizationName = "Default";
            const string defaultRoleName = "Independent";
            const string system = "System";

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
                RoleDescription = $"{defaultRoleName} stands for himself.",
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

        public override Empty Register(RegisterInput input)
        {
            if (!input.IsRemove)
            {
                foreach (var address in input.PermittedAddressList.Value)
                {
                    State.IsPermittedAddressMap[Context.Sender][input.EventId][address] = true;
                }
            }
            else
            {
                foreach (var address in input.PermittedAddressList.Value)
                {
                    State.IsPermittedAddressMap[Context.Sender][input.EventId].Remove(address);
                }
            }

            return new Empty();
        }

        public override Empty Forward(ForwardInput input)
        {
            Assert(State.IsPermittedAddressMap[input.ToAddress][input.EventId][Context.Sender], "No permission.");
            var fromHash = HashHelper.ComputeFrom($"{input.EventId}-{input.FromId}");
            Context.SendVirtualInline(fromHash, input.ToAddress, input.MethodName,
                input.Parameter);
            State.ForwardRecordMap[Context.TransactionId] = new ForwardRecord
            {
                VirtualFromAddress = Context.ConvertVirtualAddressToContractAddress(fromHash),
                ToAddress = input.ToAddress,
                FromId = input.FromId,
                MethodName = input.MethodName,
                Parameter = input.Parameter,
                EventId = input.EventId
            };
            return new Empty();
        }

        public override ForwardRecord GetForwardRecord(Hash input)
        {
            return State.ForwardRecordMap[input];
        }
    }
}