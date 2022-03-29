using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContractState : ContractState
    {
        public SingletonState<Address> DACMarketContractAddress { get; set; }

        public MappedState<string, DACCollectionInfo> DACCollectionInfoMap { get; set; }
        public MappedState<string, long, DACInfo> DACInfoMap { get; set; }

        /// <summary>
        /// DAC Name -> Owner Address -> Id List
        /// </summary>
        public MappedState<string, Address, Int64List> OwnDACListMap { get; set; }

        public MappedState<Hash, Address, long> BalanceMap { get; set; }

        /// <summary>
        /// DAC Hash -> Owner Address -> Spender Address -> Approved Amount
        /// Need to record approved by whom.
        /// </summary>
        public MappedState<Hash, Address, Address, long> AllowanceMap { get; set; }

        public MappedState<Hash, AssembledDacs> AssembledNftsMap { get; set; }
        public MappedState<Hash, AssembledCredits> AssembledFtsMap { get; set; }

        public MappedState<string, string> NFTTypeShortNameMap { get; set; }
        public MappedState<string, string> NFTTypeFullNameMap { get; set; }

        public SingletonState<DACTypes> DACTypes { get; set; }

        /// <summary>
        /// Symbol (Collection) -> Owner Address -> Operator Address List
        /// </summary>
        public MappedState<string, Address, AddressList> OperatorMap { get; set; }
    }
}