using Sinodac.Contracts.Delegator;

namespace Sinodac.Contracts.Credit
{
    public partial class CreditContractState
    {
        internal DelegatorContractContainer.DelegatorContractReferenceState DelegatorContract { get; set; }
    }
}