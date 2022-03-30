using System.Collections.Generic;
using AElf.Boilerplate.TestBase;
using AElf.Kernel.SmartContract.Application;
using AElf.Types;
using Volo.Abp.DependencyInjection;

namespace Sinodac.Contracts.DAC
{
    public class DelegatorInitializationProvider : IContractInitializationProvider
    {
        public List<ContractInitializationMethodCall> GetInitializeMethodList(byte[] contractCode)
        {
            return new List<ContractInitializationMethodCall>();
        }

        public Hash SystemSmartContractName { get; } = DelegatorSmartContractAddressNameProvider.Name;
        public string ContractCodeName { get; } = "Sinodac.Contracts.Delegator";
    }
}