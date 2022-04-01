using System.Collections.Generic;

namespace Sinodac.Contracts.Delegator.Strategies
{
    public interface IPermissionSetStrategy
    {
        List<string> ExtractPermissionList(List<string> rolePermissionList);
    }
}