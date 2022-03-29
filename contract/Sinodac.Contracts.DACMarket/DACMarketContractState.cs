using AElf.Sdk.CSharp.State;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.DACMarket
{
    public partial class DACMarketContractState : ContractState
    {
        public MappedState<string, DACSeries> DACSeriesMap { get; set; }

        public MappedState<string, ListInfo> ListInfoMap { get; set; }
    }
}