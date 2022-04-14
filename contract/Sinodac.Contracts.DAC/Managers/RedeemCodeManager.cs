using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Sinodac.Contracts.DAC.Managers
{
    public class RedeemCodeManager
    {
        private readonly CSharpSmartContractContext _context;
        private readonly MappedState<Hash, DACInfo> _redeemCodeMap;

        public RedeemCodeManager(CSharpSmartContractContext context, MappedState<Hash, DACInfo> redeemCodeMap)
        {
            _context = context;
            _redeemCodeMap = redeemCodeMap;
        }

        public void Create(string dacName, long dacId, Hash redeemCodeHash)
        {
            if (_redeemCodeMap[redeemCodeHash] != null)
            {
                throw new AssertionException($"DAC {dacName}-{dacId} 已经绑定过哈希值为 {redeemCodeHash.ToHex()} 的兑换码了。");
            }

            _redeemCodeMap[redeemCodeHash] = new DACInfo
            {
                DacName = dacName,
                DacId = dacId
            };

            _context.Fire(new RedeemCodeCreated
            {
                DacName = dacName,
                DacId = dacId,
                RedeemCodeHash = redeemCodeHash
            });
        }
    }
}