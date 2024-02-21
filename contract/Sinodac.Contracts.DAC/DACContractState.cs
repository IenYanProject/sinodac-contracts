using AElf.Contracts.MultiToken;
using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContractState : ContractState
    {
        public SingletonState<Address> DACMarketContractAddress { get; set; }
        internal TokenContractContainer.TokenContractReferenceState TokenContract { get; set; }
        public MappedState<string, DACProtocolInfo> DACProtocolInfoMap { get; set; }
        public MappedState<string, long, DACInfo> DACInfoMap { get; set; }
        public MappedState<Hash, DACInfo> RedeemCodeDACMap { get; set; }
        public MappedState<string, long, Hash> DACRedeemCodeMap { get; set; }
        public MappedState<string, long> BindRedeemCodeCountMap { get; set; }
        public MappedState<string, Address> OwnerMap { get; set; }
        public MappedState<Address, NFTInfoList> OwnerNFTMap { get; set; }
        public MappedState<string, Address, long> BalanceMap { get; set; }
        public MappedState<string, bool> IsApprovedMap { get; set; }
    }
}