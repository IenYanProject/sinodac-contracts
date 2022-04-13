using AElf.Sdk.CSharp.State;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContractState
    {
        #region Role

        /// <summary>
        /// Role Name -> Role.
        /// </summary>
        public MappedState<string, Role> RoleMap { get; set; }

        /// <summary>
        /// Role Name -> Action Id List.
        /// Only for reading, not for checking.
        /// </summary>
        public MappedState<string, StringList> RolePermissionListMap { get; set; }

        /// <summary>
        /// Role Name -> Action Id -> Is Permitted.
        /// Only for quick judgement.
        /// </summary>
        public MappedState<string, string, bool> RolePermissionMap { get; set; }

        /// <summary>
        /// Role Name -> Organization Unit List.
        /// </summary>
        public MappedState<string, StringList> RoleOrganizationUnitListMap { get; set; }

        #endregion



        #region Organization Unit & Organization Department

        /// <summary>
        /// Organization Name -> OrganizationCertificate.
        /// </summary>
        public MappedState<string, OrganizationCertificate> OrganizationCertificateMap { get; set; }

        /// <summary>
        /// Organization Name -> OrganizationUnit.
        /// </summary>
        public MappedState<string, OrganizationUnit> OrganizationUnitMap { get; set; }

        /// <summary>
        /// Department Key -> OrganizationGroup.
        /// </summary>
        public MappedState<string, OrganizationDepartment> OrganizationDepartmentMap { get; set; }

        /// <summary>
        /// Department Key -> Ignored Permission List
        /// </summary>
        public MappedState<string, StringList> OrganizationDepartmentIgnoredPermissionListMap { get; set; }

        #endregion

        #region Independent Artist
        
        public MappedState<string, IndependentArtist> IndependentArtistMap { get; set; }


        /// <summary>
        /// Independent Artist Name -> IndependentCertificate.
        /// </summary>
        public MappedState<string, IndependentCertificate> IndependentCertificateMap { get; set; }

        #endregion


        #region User

        /// <summary>
        /// User Name -> User.
        /// </summary>
        public MappedState<string, User> UserMap { get; set; }

        public MappedState<string, StringList> OrganizationUnitUserListMap { get; set; }

        #endregion
    }
}