using AElf.Sdk.CSharp;
using AElf.Types;

namespace Sinodac.Contracts.Credit
{
    public partial class CreditContract
    {
        private bool IsAddressInCreateTokenWhiteList(Address address)
        {
            return State.CreateTokenWhiteListMap[address];
        }
    }
}