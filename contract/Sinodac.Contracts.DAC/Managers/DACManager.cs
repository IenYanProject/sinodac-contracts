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
            var dacInfo = PerformCreate(dacName, dacId, redeemCodeHash);

            _context.Fire(new DACMinted
            {
                DacName = dacName,
                FromDacId = dacId,
                Quantity = 1,
                DacInfo = dacInfo
            });
        }

        private DACInfo PerformCreate(string dacName, long dacId, Hash redeemCodeHash = null)
        {
            var dacHash = DACHelper.CalculateDACHash(dacName, dacId);
            _dacMap[dacName][dacId] = new DACInfo
            {
                DacName = dacName,
                DacId = dacId,
                DacHash = dacHash,
                RedeemCodeHash = redeemCodeHash
            };
            var initialAddress = CalculateInitialAddress(dacHash);
            _ownerMap[dacName][dacId] = initialAddress;
            return _dacMap[dacName][dacId];
        }

        public void BatchCreate(string dacName, long fromDacId, long count = 0)
        {
            var protocol = _protocolManager.GetProtocol(dacName);

            count = count == 0
                ? protocol.Circulation.Sub(fromDacId).Add(1)
                : Math.Min(protocol.Circulation.Sub(fromDacId).Add(1), count);

            for (var dacId = fromDacId; dacId <= count; dacId++)
            {
                if (_dacMap[dacName][dacId] == null)
                {
                    PerformCreate(dacName, dacId);
                }
            }

            _context.Fire(new DACMinted
            {
                DacName = dacName,
                FromDacId = fromDacId,
                Quantity = count
            });
        }

        public void InitialTransfer(string dacName, long dacId, Address to)
        {
            var initialAddress = CalculateInitialAddress(DACHelper.CalculateDACHash(dacName, dacId));
            if (_ownerMap[dacName][dacId] == null)
            {
                throw new AssertionException($"DAC {dacName}:{dacId} 还没有mint到初始地址");
            }

            if (_ownerMap[dacName][dacId] != initialAddress)
            {
                throw new AssertionException($"DAC {dacName}:{dacId} 已经从初始地址转给 {_ownerMap[dacName][dacId]} 了");
            }

            _ownerMap[dacName][dacId] = to;
            _balanceMap[dacName][to] = _balanceMap[dacName][to].Add(1);

            _context.Fire(new DACInitialTransferred
            {
                DacName = dacName,
                DacId = dacId,
                From = initialAddress,
                To = to
            });
        }

        public void Transfer(string dacName, long dacId, Address from, Address to)
        {
            if (_ownerMap[dacName][dacId] != from)
            {
                throw new AssertionException($"{from} 不拥有 DAC {dacName}:{dacId}");
            }

            _ownerMap[dacName][dacId] = to;
            _balanceMap[dacName][to] = _balanceMap[dacName][to].Add(1);
            _balanceMap[dacName][from] = _balanceMap[dacName][from].Sub(1);

            _context.Fire(new DACTransferred
            {
                DacName = dacName,
                DacId = dacId,
                From = from,
                To = to
            });
        }

        private Address CalculateInitialAddress(Hash dacHash)
        {
            return _context.ConvertVirtualAddressToContractAddress(dacHash);
        }
    }
}