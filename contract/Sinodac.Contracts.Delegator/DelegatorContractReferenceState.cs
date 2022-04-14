using Sinodac.Contracts.DAC;
using Sinodac.Contracts.DACMarket;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContractState
    {
        internal DACContractContainer.DACContractReferenceState DACContract { get; set; }
        internal DACMarketContractContainer.DACMarketContractReferenceState DACMarketContract { get; set; }
    }
}