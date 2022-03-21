using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.DAC
{
    public class DACContract : DACContractContainer.DACContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            return new Empty();
        }
    }
}