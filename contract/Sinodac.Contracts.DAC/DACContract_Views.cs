using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContract
    {
        public override DACBalance GetBalance(GetBalanceInput input)
        {
            return base.GetBalance(input);
        }

        public override DACAllowance GetAllowance(GetAllowanceInput input)
        {
            return base.GetAllowance(input);
        }

        public override Hash CalculateDACHash(CalculateDACHashInput input)
        {
            return base.CalculateDACHash(input);
        }
    }
}