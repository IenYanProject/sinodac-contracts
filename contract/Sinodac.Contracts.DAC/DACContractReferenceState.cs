using AElf.Standards.ACS6;
using Sinodac.Contracts.Credit;
using Sinodac.Contracts.Delegator;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContractState
    {
        internal DelegatorContractContainer.DelegatorContractReferenceState DelegatorContract { get; set; }

        internal RandomNumberProviderContractContainer.RandomNumberProviderContractReferenceState
            RandomNumberProviderContract { get; set; }

        internal CreditContractContainer.CreditContractReferenceState CreditContract { get; set; }
    }
}