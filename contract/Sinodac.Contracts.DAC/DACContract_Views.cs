using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using Sinodac.Contracts.DAC.Helpers;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContract
    {
        public override DACBalance GetBalance(GetBalanceInput input)
        {
            return new DACBalance
            {
                DacName = input.DacName,
                Owner = input.Owner,
                Balance = State.BalanceMap[input.DacName][input.Owner]
            };
        }

        public override Hash CalculateDACHash(CalculateDACHashInput input)
        {
            return DACHelper.CalculateDACHash(input.DacName, input.DacId);
        }
    }
}