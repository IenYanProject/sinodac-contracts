using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Sinodac.Contracts.Credit
{
    public class CreditContractState : ContractState
    {
        public SingletonState<Address> Admin { get; set; }
        public MappedState<Address, bool> CreateTokenWhiteListMap { get; set; }
        public MappedState<string, CreditInfo> CreditInfoMap { get; set; }
    }
}