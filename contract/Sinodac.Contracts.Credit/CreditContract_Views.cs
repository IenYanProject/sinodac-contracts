using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Credit
{
    public partial class CreditContract
    {
        public override CreditBalance GetBalance(GetBalanceInput input)
        {
            return new CreditBalance
            {
                Owner = input.Owner,
                Symbol = input.Symbol,
                Balance = State.BalanceMap[input.Owner][input.Symbol]
            };
        }

        public override CreditAllowance GetAllowance(GetAllowanceInput input)
        {
            return new CreditAllowance
            {
                Owner = input.Owner,
                Spender = input.Spender,
                Symbol = input.Symbol,
                Allowance = State.AllowanceMap[input.Owner][input.Spender][input.Symbol]
            };
        }

        public override CreditInfo GetCreditInfo(GetCreditInfoInput input)
        {
            return State.CreditInfoMap[input.Symbol];
        }

        public override BoolValue IsInCreateCreditWhiteList(Address input)
        {
            return new BoolValue
            {
                Value = State.CreateCreditWhiteListMap[input]
            };
        }
    }
}