using AElf;
using AElf.CSharp.Core;
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

            State.DACContract.Value = input.DacContractAddress;

            State.DACContract.Initialize.Send(new Contracts.DAC.InitializeInput
            {
                AdminAddress = State.Admin.Value,
                DelegatorContractAddress = Context.Self,
            });

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

        public override Empty Forward(ForwardInput input)
        {
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
            State.TemporaryTxIdMap[Context.TransactionId] = 1;
            return new Empty();
        }

        public override Empty ForwardCheck(Hash input)
        {
            Assert(State.TemporaryTxIdMap[input] > 0, "Forward check failed.");
            State.TemporaryTxIdMap[input] = State.TemporaryTxIdMap[input].Sub(1);
            if (State.TemporaryTxIdMap[input] == 0)
            {
                State.TemporaryTxIdMap.Remove(input);
            }

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

        public override Empty EnablePermissionCheck(EnablePermissionCheckInput input)
        {
            State.EnablePermissionCheck.Value = input.Enable;
            return new Empty();
        }
    }
}