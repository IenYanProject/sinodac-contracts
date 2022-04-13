using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContractState : ContractState
    {
        public SingletonState<Address> DACMarketContractAddress { get; set; }

        public MappedState<string, DACProtocolInfo> DACProtocolInfoMap { get; set; }
        public MappedState<string, long, DACInfo> DACInfoMap { get; set; }
        public MappedState<string, long, Hash> RedeemCodeMap { get; set; }
        public MappedState<string, long, Address> OwnerMap { get; set; }
        public MappedState<string, Address, long> BalanceMap { get; set; }
        public MappedState<string, bool> IsApprovedMap { get; set; }
    }
}