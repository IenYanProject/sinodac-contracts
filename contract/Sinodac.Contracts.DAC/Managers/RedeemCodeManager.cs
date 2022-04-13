using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Sinodac.Contracts.DAC.Managers
{
    public class RedeemCodeManager
    {
        private readonly CSharpSmartContractContext _context;
        private readonly MappedState<string, long, Hash> _redeemCodeMap;

        public RedeemCodeManager(CSharpSmartContractContext context, MappedState<string, long, Hash> redeemCodeMap)
        {
            _context = context;
            _redeemCodeMap = redeemCodeMap;
        }

        public void Create(string dacName, long dacId, Hash redeemCodeHash)
        {
            if (_redeemCodeMap[dacName][dacId] != null)
            {
                throw new AssertionException($"DAC {dacName}-{dacId} 已经绑定过哈希值为 {redeemCodeHash.ToHex()} 的兑换码了。");
            }

            _redeemCodeMap[dacName][dacId] = redeemCodeHash;
        }
    }
}