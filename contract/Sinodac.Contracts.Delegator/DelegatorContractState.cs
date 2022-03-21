using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Sinodac.Contracts.Delegator
{
    public class DelegatorContractState : ContractState
    {
        /// <summary>
        /// To Address -> Event Id -> Sender Address -> Is Permitted.
        /// </summary>
        public MappedState<Address, string, Address, bool> IsPermittedAddressMap { get; set; }
        public MappedState<Hash, ForwardRecord> ForwardRecordMap { get; set; }
    }
}