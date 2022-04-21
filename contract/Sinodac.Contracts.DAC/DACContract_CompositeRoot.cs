using Sinodac.Contracts.DAC.Managers;
using Sinodac.Contracts.DAC.Services;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContract
    {
        private ProtocolManager GetProtocolManager()
        {
            return new ProtocolManager(Context, State.DACProtocolInfoMap, State.IsApprovedMap);
        }

        private DACManager GetDACManager(ProtocolManager protocolManager = null)
        {
            if (protocolManager == null)
            {
                protocolManager = GetProtocolManager();
            }

            return new DACManager(Context, protocolManager, State.DACInfoMap, State.OwnerMap, State.BalanceMap);
        }

        private RedeemCodeManager GetRedeemCodeManager()
        {
            return new RedeemCodeManager(Context, State.RedeemCodeDACMap, State.DACRedeemCodeMap,
                State.BindRedeemCodeCountMap);
        }

        private DACService GetDACService()
        {
            return new DACService(Context, GetProtocolManager(), GetDACManager(), GetRedeemCodeManager());
        }
    }
}