using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Sinodac.Contracts.Delegator
{
    public class DelegatorContractState : ContractState
    {
        public SingletonState<Address> Admin { get; set; }

        /// <summary>
        /// To Address -> Scope Id -> Sender Address -> Is Permitted.
        /// </summary>
        public MappedState<Address, string, Address, bool> IsPermittedAddressMap { get; set; }

        /// <summary>
        /// To Address -> Scope Id -> Method Name -> Is Permitted.
        /// </summary>
        public MappedState<Address, string, string, bool> IsPermittedMethodNameMap { get; set; }

        /// <summary>
        /// Tx Id -> ForwardRecord.
        /// </summary>
        public MappedState<Hash, ForwardRecord> ForwardRecordMap { get; set; }

        public MappedState<Hash, bool> TemporaryTxIdMap { get; set; }

        /// <summary>
        /// Role Name -> Action Id -> Is Permitted.
        /// </summary>
        public MappedState<string, string, bool> RolePermissionMap { get; set; }

        /// <summary>
        /// Role Name -> Action Id List (Stands For Permissions).
        /// Only for reading, not for checking.
        /// </summary>
        public MappedState<string, StringList> RoleActionIdListMap { get; set; }

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

        public MappedState<string, IndependentArtist> IndependentArtistMap { get; set; }

        /// <summary>
        /// Role Name -> Organization Unit List.
        /// </summary>
        public MappedState<string, StringList> RoleOrganizationUnitListMap { get; set; }

        public MappedState<string, StringList> OrganizationUnitUserListMap { get; set; }

        /// <summary>
        /// Organization Name -> OrganizationCertificate.
        /// </summary>
        public MappedState<string, OrganizationCertificate> OrganizationCertificateMap { get; set; }

        /// <summary>
        /// Independent Artist Name -> IndependentCertificate.
        /// </summary>
        public MappedState<string, IndependentCertificate> IndependentCertificateMap { get; set; }

        /// <summary>
        /// Group Key -> Permission List
        /// </summary>
        public MappedState<string, StringList> GroupActionIdListMap { get; set; }

        /// <summary>
        /// Group Key -> OrganizationGroup
        /// </summary>
        public MappedState<string, OrganizationGroup> OrganizationGroupMap { get; set; }
    }
}