using System.Collections.Generic;
using AElf.Boilerplate.TestBase;
using AElf.ContractTestBase;
using AElf.Kernel.SmartContract.Application;
using AElf.Types;

namespace Sinodac.Contracts.DAC
{
    public class MainChainDAppContractTestDeploymentListProvider : MainChainContractDeploymentListProvider,
        IContractDeploymentListProvider
    {
        public new List<Hash> GetDeployContractNameList()
        {
            var list = base.GetDeployContractNameList();
            list.Add(DAppSmartContractAddressNameProvider.Name);
            list.Add(DelegatorSmartContractAddressNameProvider.Name);
            return list;
        }
    }
}