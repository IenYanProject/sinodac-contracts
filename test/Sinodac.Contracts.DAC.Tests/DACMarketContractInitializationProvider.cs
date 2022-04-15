using System.Collections.Generic;
using AElf.Boilerplate.TestBase;
using AElf.Kernel.SmartContract.Application;
using AElf.Types;

namespace Sinodac.Contracts.DAC
{
    public class DACMarketContractInitializationProvider : IContractInitializationProvider
    {
        public List<ContractInitializationMethodCall> GetInitializeMethodList(byte[] contractCode)
        {
            return new List<ContractInitializationMethodCall>();
        }

        public Hash SystemSmartContractName { get; } = DACMarketSmartContractAddressNameProvider.Name;
        public string ContractCodeName { get; } = "Sinodac.Contracts.DACMarket";
    }
}