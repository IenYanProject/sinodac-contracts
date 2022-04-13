using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContract : DACContractContainer.DACContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            PerformRegisterPermissions(input);
            State.DACMarketContractAddress.Value = input.DacMarketContractAddress;
            return new Empty();
        }
    }
}