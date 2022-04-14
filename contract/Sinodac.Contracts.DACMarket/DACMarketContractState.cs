using AElf.Sdk.CSharp.State;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using Sinodac.Contracts.Delegator;

namespace Sinodac.Contracts.DACMarket
{
    public partial class DACMarketContractState : ContractState
    {
        public MappedState<string, DACSeries> DACSeriesMap { get; set; }

        public MappedState<string, Timestamp> PublicTimeMap { get; set; }

        public MappedState<Address, StringList> OwnBoxCodeMap { get; set; }

        public MappedState<string, string> BoxIdSeedMap { get; set; }

        public MappedState<string, BoxInfo> BoxInfoMap { get; set; }
    }
}