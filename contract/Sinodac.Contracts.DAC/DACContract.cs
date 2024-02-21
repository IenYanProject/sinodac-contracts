using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContract : DACContractContainer.DACContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            PerformRegisterPermissions(input);
            State.TokenContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            return new Empty();
        }
    }
}