using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.DACMinter
{
    public class DACMinterContract : DACMinterContractContainer.DACMinterContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            return new Empty();
        }
    }
}