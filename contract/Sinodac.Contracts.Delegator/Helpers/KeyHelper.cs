namespace Sinodac.Contracts.Delegator.Helpers
{
    public static class KeyHelper
    {
        public static string GetOrganizationDepartmentKey(string organizationName, string departmentName)
        {
            return $"{organizationName}-{departmentName}";
        }
    }
}