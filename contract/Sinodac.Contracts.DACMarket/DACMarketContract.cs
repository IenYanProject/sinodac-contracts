using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.DACMarket
{
    public class DACMarketContract : DACMarketContractContainer.DACMarketContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            return new Empty();
        }
    }
}