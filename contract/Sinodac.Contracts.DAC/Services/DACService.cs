using System.Collections.Generic;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using AElf.Types;
using Sinodac.Contracts.DAC.Managers;

namespace Sinodac.Contracts.DAC.Services
{
    public class DACService
    {
        private readonly CSharpSmartContractContext _context;
        private readonly DACManager _dacManager;

        public DACService(CSharpSmartContractContext context, DACManager dacManager)
        {
            _context = context;
            _dacManager = dacManager;
        }

        /// <summary>
        /// 批量Mint
        /// </summary>
        /// <param name="dacName"></param>
        /// <param name="fromDacId"></param>
        /// <param name="dacFile"></param>
        /// <param name="count"></param>
        public void BatchMint(string dacName, long fromDacId, Hash dacFile, long count, long circulation)
        {
            _dacManager.BatchCreate(dacName, fromDacId, dacFile, count, circulation);
        }

        /// <summary>
        /// 用户购买或兑换成功后调用该方法，将DAC从初始地址转给用户
        /// </summary>
        /// <param name="dacName"></param>
        /// <param name="dacId"></param>
        /// <param name="to"></param>
        public void InitialTransfer(string nftInfoId, Address to, string nftHash, string nftFile, string owner, string nftName, long nftId)
        {
            _dacManager.InitialTransfer(nftInfoId, to, nftHash, nftFile, owner, nftName, nftId);
        }
    }
}