using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Credit
{
    public class CreditContract : CreditContractContainer.CreditContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            return new Empty();
        }
    }
}