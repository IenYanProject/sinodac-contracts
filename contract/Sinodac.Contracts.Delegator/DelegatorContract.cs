using AElf;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public class DelegatorContract : DelegatorContractContainer.DelegatorContractBase
    {
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