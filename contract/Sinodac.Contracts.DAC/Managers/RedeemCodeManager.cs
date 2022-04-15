using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace Sinodac.Contracts.DAC.Managers
{
    public class RedeemCodeManager
    {
        private readonly CSharpSmartContractContext _context;
        private readonly MappedState<Hash, DACInfo> _redeemCodeMap;
        private readonly MappedState<string, long> _countMap;

        public RedeemCodeManager(CSharpSmartContractContext context, MappedState<Hash, DACInfo> redeemCodeMap,
            MappedState<string, long> countMap)
        {
            _context = context;
            _redeemCodeMap = redeemCodeMap;
            _countMap = countMap;
        }

        public void Create(string dacName, long dacId, Hash redeemCodeHash)
        {
            if (_redeemCodeMap[redeemCodeHash] != null)
            {
                throw new AssertionException($"DAC {dacName}:{dacId} 已经绑定过哈希值为 {redeemCodeHash.ToHex()} 的兑换码了。");
            }

            _redeemCodeMap[redeemCodeHash] = new DACInfo
            {
                DacName = dacName,
                DacId = dacId
            };

            _countMap[dacName] = _countMap[dacName].Add(1);

            _context.Fire(new RedeemCodeCreated
            {
                DacName = dacName,
                DacId = dacId,
                RedeemCodeHash = redeemCodeHash
            });
        }

        public long GetBindCount(string dacName)
        {
            return _countMap[dacName];
        }
    }
}