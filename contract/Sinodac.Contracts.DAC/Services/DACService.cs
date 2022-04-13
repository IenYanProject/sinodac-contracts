using System.Collections.Generic;
using AElf.Sdk.CSharp;
using AElf.Types;
using Sinodac.Contracts.DAC.Managers;

namespace Sinodac.Contracts.DAC.Services
{
    public class DACService
    {
        private readonly CSharpSmartContractContext _context;
        private readonly ProtocolManager _protocolManager;
        private readonly DACManager _dacManager;
        private readonly RedeemCodeManager _redeemCodeManager;

        public DACService(CSharpSmartContractContext context, ProtocolManager protocolManager, DACManager dacManager,
            RedeemCodeManager redeemCodeManager)
        {
            _context = context;
            _protocolManager = protocolManager;
            _dacManager = dacManager;
            _redeemCodeManager = redeemCodeManager;
        }

        public void CreateProtocol(DACProtocolInfo protocol)
        {
            _protocolManager.Create(protocol);
        }

        /// <summary>
        /// 上架后，批量Mint
        /// 调用前需要判断该DAC是否审核
        /// </summary>
        /// <param name="dacName"></param>
        /// <param name="fromDacId"></param>
        /// <param name="count"></param>
        public void BatchMint(string dacName, long fromDacId, long count = 0)
        {
            _dacManager.BatchCreate(dacName, fromDacId, count);
        }

        /// <summary>
        /// 后端生成兑换码，通过调用该方法绑定兑换码并创建出DAC
        /// 注意少量多次
        /// </summary>
        /// <param name="dacName"></param>
        /// <param name="redeemCodeHashMap"></param>
        public void BindRedeemCode(string dacName, Dictionary<long, Hash> redeemCodeHashMap)
        {
            foreach (var pair in redeemCodeHashMap)
            {
                var dacId = pair.Key;
                var redeemCodeHash = pair.Value;
                _redeemCodeManager.Create(dacName, dacId, redeemCodeHash);
            }
        }

        /// <summary>
        /// 用户购买或兑换成功后调用该方法，将DAC从初始地址转给用户
        /// </summary>
        /// <param name="dacName"></param>
        /// <param name="dacId"></param>
        /// <param name="to"></param>
        public void InitialTransfer(string dacName, long dacId, Address to)
        {
            _dacManager.InitialTransfer(dacName, dacId, to);
        }

        /// <summary>
        /// 用户转赠DAC
        /// </summary>
        /// <param name="dacName"></param>
        /// <param name="dacId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void Transfer(string dacName, long dacId, Address from, Address to)
        {
            _dacManager.Transfer(dacName, dacId, from, to);
        }

        /// <summary>
        /// 审核通过DAC
        /// </summary>
        /// <param name="dacName"></param>
        public void ApproveProtocol(string dacName)
        {
            _protocolManager.Approve(dacName);
        }
    }
}