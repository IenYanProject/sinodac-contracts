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
        private readonly MappedState<string, long, DACInfo> _dacMap;
        private readonly MappedState<string, Address> _ownerMap;
        private readonly MappedState<string, Address, long> _balanceMap;
        private readonly MappedState<Address, NFTInfoList> _ownerNftMap;

        public DACManager(CSharpSmartContractContext context,
            MappedState<string, long, DACInfo> dacMap,
            MappedState<string, Address> ownerMap,
            MappedState<string, Address, long> balanceMap, MappedState<Address, NFTInfoList> ownerNftMap)
        {
            _context = context;
            _dacMap = dacMap;
            _ownerMap = ownerMap;
            _balanceMap = balanceMap;
            _ownerNftMap = ownerNftMap;
        }

        private DACInfo PerformCreate(string dacName, long dacId, Hash dacFile = null)
        {
            var dacHash = DACHelper.CalculateDACHash(dacName, dacId);
            _dacMap[dacName][dacId] = new DACInfo
            {
                DacName = dacName,
                DacId = dacId,
                DacHash = dacHash,
                DacFile = dacFile,
                //RedeemCodeHash = redeemCodeHash
            };
            var initialAddress = CalculateInitialAddress(dacHash);

            var strHash = dacHash.ToString();
            _ownerMap[strHash] = initialAddress;
            return _dacMap[dacName][dacId];
        }
        
        public void BatchCreate(string dacName, long fromDacId, Hash dacFile, long count = 0, long circulation = 0)
        {
            count = count == 0
                ? circulation.Sub(fromDacId).Add(1)
                : Math.Min(circulation.Sub(fromDacId).Add(1), count);

            var dacMintInfo = new DACInfoList();
            
            for (long dacId = fromDacId; dacId < count.Add(fromDacId); dacId++)
            {
                if (_dacMap[dacName][dacId] == null)
                {
                    var dacInfo = PerformCreate(dacName, dacId, dacFile);
                    dacMintInfo.Value.Add(dacInfo);
                }
            }

            _context.Fire(new DACMinted()
            {
                DacName = dacName,
                FromDacId = fromDacId,
                Quantity = count,
                DacInfo = dacMintInfo,
                ContractAddress = _context.Self
            });
        }
        public void InitialTransfer(string nftInfoId, Address to, string nftHash, string nftFile, string owner, string nftName, long nftId)
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

            var nftInfo = _dacMap[nftName][nftId];
            _ownerNftMap[to].NftInfo.Add(new NftInfo()
            {
                DacHash = nftInfo.DacHash,
                DacFile = nftInfo.DacFile,
                DacId = nftInfo.DacId,
                DacName = nftInfo. DacName
            });
            
            _context.Fire(new DACInitialTransferred
            {
                NftInfoId = nftInfoId,
                From = initialAddress,
                To = to,
                Owner = owner,
                ContractAddress = _context.Self
            });
        }

        private Address CalculateInitialAddress(Hash dacHash)
        {
            return _context.ConvertVirtualAddressToContractAddress(dacHash);
        }
    }
}