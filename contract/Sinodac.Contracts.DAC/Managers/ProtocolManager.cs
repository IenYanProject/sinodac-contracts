using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;

namespace Sinodac.Contracts.DAC.Managers
{
    /// <summary>
    /// 一个Protocol代表一类藏品
    /// 1. Metadata暂不使用
    /// </summary>
    public class ProtocolManager
    {
        private readonly CSharpSmartContractContext _context;
        private readonly MappedState<string, DACProtocolInfo> _protocolMap;
        private readonly MappedState<string, bool> _isApprovedMap;

        public ProtocolManager(CSharpSmartContractContext context,
            MappedState<string, DACProtocolInfo> protocolMap,
            MappedState<string, bool> isApprovedMap)
        {
            _context = context;
            _protocolMap = protocolMap;
            _isApprovedMap = isApprovedMap;
        }

        public void Create(DACProtocolInfo protocol)
        {
            var dacName = protocol.DacName;
            if (_protocolMap[protocol.DacName] != null)
            {
                throw new AssertionException($"藏品 {dacName} 已经存在了");
            }

            _protocolMap[protocol.DacName] = protocol;

            _context.Fire(new DACProtocolCreated
            {
                DacName = protocol.DacName,
                DacProtocolInfo = protocol
            });
        }

        public void Approve(string dacName)
        {
            _isApprovedMap[dacName] = true;
            
            _context.Fire(new DACProtocolApproved
            {
                DacName = dacName
            });
        }

        public bool IsApproved(string dacName)
        {
            return _isApprovedMap[dacName];
        }

        public void AssertProtocolExists(string dacName)
        {
            if (_protocolMap[dacName] == null)
            {
                throw new AssertionException($"藏品 {dacName} 不存在");
            }
        }

        public DACProtocolInfo GetProtocol(string dacName)
        {
            AssertProtocolExists(dacName);
            return _protocolMap[dacName];
        }
    }
}