using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContractState : ContractState
    {
        public SingletonState<Address> Admin { get; set; }

        /// <summary>
        /// To Address -> Scope Id -> Sender Address -> Is Permitted.
        /// </summary>
        public MappedState<Address, string, Address, bool> IsPermittedAddressMap { get; set; }

        /// <summary>
        /// To Address -> Scope Id -> Method Name -> Is Permitted.
        /// </summary>
        public MappedState<Address, string, string, bool> IsPermittedMethodNameMap { get; set; }

        public MappedState<Hash, long> TemporaryTxIdMap { get; set; }

        /// <summary>
        /// Tx Id -> ForwardRecord.
        /// </summary>
        public MappedState<Hash, ForwardRecord> ForwardRecordMap { get; set; }

        public BoolState EnablePermissionCheck { get; set; }
    }
}