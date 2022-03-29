using Sinodac.Contracts.DAC;
using Sinodac.Contracts.Delegator;

namespace Sinodac.Contracts.DACMarket
{
    public partial class DACMarketContractState
    {
        internal DelegatorContractContainer.DelegatorContractReferenceState DelegatorContract { get; set; }
        internal DACContractContainer.DACContractReferenceState DACContract { get; set; }
    }
}