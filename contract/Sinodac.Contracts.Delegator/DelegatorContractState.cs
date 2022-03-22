using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Sinodac.Contracts.Delegator
{
    public class DelegatorContractState : ContractState
    {
        public SingletonState<Address> Admin { get; set; }

        /// <summary>
        /// To Address -> Event Id -> Sender Address -> Is Permitted.
        /// </summary>
        public MappedState<Address, string, Address, bool> IsPermittedAddressMap { get; set; }

        /// <summary>
        /// Tx Id -> Forward Record (History).
        /// </summary>
        public MappedState<Hash, ForwardRecord> ForwardRecordMap { get; set; }

        /// <summary>
        /// Role Name -> Action Id -> Is Permitted.
        /// </summary>
        public MappedState<string, string, bool> RolePermissionMap { get; set; }

        /// <summary>
        /// Role Name -> Role.
        /// </summary>
        public MappedState<string, Role> RoleMap { get; set; }

        /// <summary>
        /// User Name -> User.
        /// </summary>
        public MappedState<string, User> UserMap { get; set; }

        /// <summary>
        /// Organization Name -> OrganizationUnit.
        /// </summary>
        public MappedState<string, OrganizationUnit> OrganizationUnitMap { get; set; }

        /// <summary>
        /// Role Name -> Organization Unit List.
        /// </summary>
        public MappedState<string, StringList> RoleOrganizationUnitListMap { get; set; }

        public MappedState<string, StringList> OrganizationUnitUserListMap { get; set; }
    }
}