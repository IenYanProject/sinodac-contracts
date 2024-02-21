using Sinodac.Contracts.DAC.Managers;
using Sinodac.Contracts.DAC.Services;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContract
    {

        private DACManager GetDACManager()
        {
            return new DACManager(Context, State.DACInfoMap, State.OwnerMap, State.BalanceMap, State.OwnerNFTMap);
        }

        private DACService GetDACService()
        {
            return new DACService(Context, GetDACManager());
        }
    }
}