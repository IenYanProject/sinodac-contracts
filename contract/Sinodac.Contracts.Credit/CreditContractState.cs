using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Sinodac.Contracts.Credit
{
    public partial class CreditContractState : ContractState
    {
        public SingletonState<Address> Admin { get; set; }
        public MappedState<Address, bool> CreateCreditWhiteListMap { get; set; }

        /// <summary>
        /// Credit Symbol -> CreditInfo.
        /// </summary>
        public MappedState<string, CreditInfo> CreditInfoMap { get; set; }

        /// <summary>
        /// Credit Symbol -> Owner Virtual Address -> Balance.
        /// </summary>
        public MappedState<Address, string, long> BalanceMap { get; set; }

        /// <summary>
        /// Owner Virtual Address -> Spender Virtual Address -> Credit Symbol -> Allowance.
        /// </summary>
        public MappedState<Address, Address, string, long> AllowanceMap { get; set; }
    }
}