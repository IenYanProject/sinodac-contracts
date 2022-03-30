using System.Collections.Generic;
using System.IO;
using AElf.Boilerplate.TestBase;
using AElf.ContractTestBase;
using AElf.Kernel.SmartContract.Application;
using Microsoft.Extensions.DependencyInjection;
using Sinodac.Contracts.Delegator;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace Sinodac.Contracts.DAC
{
    [DependsOn(typeof(MainChainDAppContractTestModule))]
    public class DACContractTestModule : MainChainDAppContractTestModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IContractInitializationProvider, DACContractInitializationProvider>();
            context.Services.AddSingleton<IContractInitializationProvider, DelegatorInitializationProvider>();
            context.Services.AddSingleton<IContractDeploymentListProvider, MainChainDAppContractTestDeploymentListProvider>();
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            var contractCodeProvider = context.ServiceProvider.GetService<IContractCodeProvider>();
            var contractCodes = new Dictionary<string, byte[]>(contractCodeProvider.Codes)
            {
                {
                    new DACContractInitializationProvider().ContractCodeName,
                    File.ReadAllBytes(typeof(DACContract).Assembly.Location)
                },
                {
                    new DelegatorInitializationProvider().ContractCodeName,
                    File.ReadAllBytes(typeof(DelegatorContract).Assembly.Location)
                }
            };
            contractCodeProvider.Codes = contractCodes;
        }
    }
}