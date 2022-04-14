using System;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;
using AElf.Types;
using Sinodac.Contracts.DAC.Helpers;

namespace Sinodac.Contracts.DAC.Managers
{
    public class DACManager
    {
        private readonly CSharpSmartContractContext _context;
        private readonly ProtocolManager _protocolManager;
        private readonly MappedState<string, long, DACInfo> _dacMap;
        private readonly MappedState<string, long, Address> _ownerMap;
        private readonly MappedState<string, Address, long> _balanceMap;

        public DACManager(CSharpSmartContractContext context, ProtocolManager protocolManager,
            MappedState<string, long, DACInfo> dacMap,
            MappedState<string, long, Address> ownerMap,
            MappedState<string, Address, long> balanceMap)
        {
            _context = context;
            _protocolManager = protocolManager;
            _dacMap = dacMap;
            _ownerMap = ownerMap;
            _balanceMap = balanceMap;
        }

        public void Create(string dacName, long dacId, Hash redeemCodeHash = null)
        {
            var dacHash = DACHelper.CalculateDACHash(dacName, dacId);
            _dacMap[dacName][dacId] = new DACInfo
            {
                DacName = dacName,
                DacId = dacId,
                DacHash = dacHash,
                RedeemCodeHash = redeemCodeHash
            };
            _ownerMap[dacName][dacId] = CalculateInitialAddress(dacHash);
        }

        public void BatchCreate(string dacName, long fromDacId, long count = 0)
        {
            var protocol = _protocolManager.GetProtocol(dacName);

            count = count == 0
                ? protocol.Circulation.Sub(fromDacId)
                : Math.Min(protocol.Circulation.Sub(fromDacId), count);

            for (var dacId = fromDacId; dacId <= count; dacId++)
            {
                if (_dacMap[dacName][dacId] == null)
                {
                    Create(dacName, dacId);
                }
            }
        }

        public void InitialTransfer(string dacName, long dacId, Address to)
        {
            var initialAddress = CalculateInitialAddress(DACHelper.CalculateDACHash(dacName, dacId));
            if (_ownerMap[dacName][dacId] != initialAddress)
            {
                throw new AssertionException($"DAC {dacName}-{dacId} 已经从初始地址转给 {_ownerMap[dacName][dacId]} 了");
            }

            _ownerMap[dacName][dacId] = to;
            _balanceMap[dacName][to] = _balanceMap[dacName][to].Add(1);
        }

        public void Transfer(string dacName, long dacId, Address from, Address to)
        {
            if (_ownerMap[dacName][dacId] != from)
            {
                throw new AssertionException($"{from} 不拥有 DAC {dacName}-{dacId}");
            }

            _ownerMap[dacName][dacId] = to;
            _balanceMap[dacName][to] = _balanceMap[dacName][to].Add(1);
            _balanceMap[dacName][from] = _balanceMap[dacName][from].Sub(1);
        }

        private Address CalculateInitialAddress(Hash dacHash)
        {
            return _context.ConvertVirtualAddressToContractAddress(dacHash);
        }
    }
}