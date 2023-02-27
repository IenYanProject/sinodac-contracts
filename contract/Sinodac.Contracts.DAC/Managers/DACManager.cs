using System;
using System.Collections.Generic;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;
using AElf.Types;
using Google.Protobuf.Collections;
using Sinodac.Contracts.DAC.Helpers;

namespace Sinodac.Contracts.DAC.Managers
{
    public class DACManager
    {
        private readonly CSharpSmartContractContext _context;
        private readonly ProtocolManager _protocolManager;
        private readonly MappedState<string, long, DACInfo> _dacMap;
        private readonly MappedState<string, Address> _ownerMap;
        private readonly MappedState<string, Address, long> _balanceMap;

        public DACManager(CSharpSmartContractContext context, ProtocolManager protocolManager,
            MappedState<string, long, DACInfo> dacMap,
            MappedState<string, Address> ownerMap,
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

            var dacInfoList = new DACInfoList();
            dacInfoList.Value.Add(dacInfo);
            
            _context.Fire(new DACMinted
            {
                DacName = dacName,
                FromDacId = dacId,
                Quantity = 1,
                DacInfo = dacInfoList
            });
        }

        private DACInfo PerformCreate(string dacName, long dacId, Hash dacFile = null)
        {
            var dacHash = DACHelper.CalculateDACHash(dacName, dacId);
            _dacMap[dacName][dacId] = new DACInfo
            {
                DacName = dacName,
                DacId = dacId,
                DacHash = dacHash,
                //DacFile = dacFile,
                //RedeemCodeHash = redeemCodeHash
            };
            var initialAddress = CalculateInitialAddress(dacHash);

            var strHash = dacHash.ToString();
            _ownerMap[strHash] = initialAddress;
            return _dacMap[dacName][dacId];
        }
        private DACInfo PerformCreate(string dacName, long dacId,string batchId)
        {
            var dacHash = DACHelper.CalculateDACHash(dacName, dacId, batchId);
            var dacInfo = new DACInfo
            {
                DacName = dacName,
                DacId = dacId,
                DacHash = dacHash,
                BatchId = batchId
            };
            var initialAddress = CalculateInitialAddress(dacHash);

            var strHash = dacHash.ToString();
            _ownerMap[strHash] = initialAddress;
            return dacInfo;
        }
       
        // public void BatchCreate(string dacName, long fromDacId, List<Hash> redeemCodeHashList, long count = 0)
        // {
        //     var protocol = _protocolManager.GetProtocol(dacName);
        //
        //     count = count == 0
        //         ? protocol.Circulation.Sub(fromDacId).Add(1)
        //         : Math.Min(protocol.Circulation.Sub(fromDacId).Add(1), count);
        //
        //     var dacMintInfo = new DACInfoList();
        //     
        //     for (long dacId = fromDacId; dacId < count.Add(fromDacId); dacId++)
        //     {
        //         if (_dacMap[dacName][dacId] == null)
        //         {
        //             var dacInfo = PerformCreate(dacName, dacId, redeemCodeHashList[(int)(dacId - fromDacId)]);
        //             dacMintInfo.Value.Add(dacInfo);
        //         }
        //     }
        //
        //     _context.Fire(new DACMinted()
        //     {
        //         DacName = dacName,
        //         FromDacId = fromDacId,
        //         Quantity = count,
        //         DacInfo = dacMintInfo
        //     });
        // }
        public void BatchCreate(string dacName, long fromDacId, long count ,string batchId,string protocolId)
        {
           
            var dacMintInfo = new DACInfoList();
            
            for (long dacId = fromDacId; dacId < count.Add(fromDacId); dacId++)
            {
                
                var dacInfo = PerformCreate(dacName, dacId,batchId);
                dacMintInfo.Value.Add(dacInfo);
                
            }

            _context.Fire(new DACMinted()
            {
                DacName = dacName,
                FromDacId = fromDacId,
                Quantity = count,
                DacInfo = dacMintInfo,
                ProtocolId = protocolId,
            });
        }
        public void InitialTransfer(string nftInfoId, Address to, string nftHash, string nftFile, string owner)
        {
            var initialAddress = _ownerMap[nftHash];
            // var initialAddress = CalculateInitialAddress(nftHash);
            if (_ownerMap[nftHash] == null)
            {
                throw new AssertionException($"NFT {nftInfoId} 还没有mint到初始地址");
            }
            
            if (_ownerMap[nftHash] != initialAddress)
            {
                throw new AssertionException($"NFT {nftInfoId} 已经从初始地址转给 {_ownerMap[nftHash]} 了");
            }

            _ownerMap[nftHash] = to;
            _balanceMap[nftFile][to] = _balanceMap[nftFile][to].Add(1);

            _context.Fire(new DACInitialTransferred
            {
                NftInfoId = nftInfoId,
                From = initialAddress,
                To = to,
                Owner = owner
            });
        }

        public void Transfer(string dacName, long dacId, Address from, Address to)
        {
            // if (_ownerMap[dacName][dacId] != from)
            // {
            //     throw new AssertionException($"{from} 不拥有 DAC {dacName}:{dacId}");
            // }
            //
            // _ownerMap[dacName][dacId] = to;
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