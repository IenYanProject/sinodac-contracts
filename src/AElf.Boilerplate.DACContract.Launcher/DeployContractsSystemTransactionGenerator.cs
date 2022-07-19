using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Standards.ACS0;
using AElf.Boilerplate.SystemTransactionGenerator;
using AElf.ContractDeployer;
using AElf.Contracts.Genesis;
using AElf.Kernel;
using AElf.Kernel.Miner.Application;
using AElf.Kernel.SmartContract;
using AElf.Types;
using Google.Protobuf;
using Microsoft.Extensions.Options;

namespace AElf.Boilerplate.DACContract.Launcher
{
    public class DeployContractsSystemTransactionGenerator : ISystemTransactionGenerator
    {
        private readonly ITransactionGeneratingService _transactionGeneratingService;
        private readonly ContractOptions _contractOptions;

        public DeployContractsSystemTransactionGenerator(ITransactionGeneratingService transactionGeneratingService,
            IOptionsSnapshot<ContractOptions> contractOptions)
        {
            _transactionGeneratingService = transactionGeneratingService;
            _contractOptions = contractOptions.Value;
        }

        public async Task<List<Transaction>> GenerateTransactionsAsync(Address @from, long preBlockHeight,
            Hash preBlockHash)
        {
            if (preBlockHeight == 1)
            {
                string[] ContractCodeKeys = new[]
                {
                    "Sinodac.Contracts.DAC",
                    "Sinodac.Contracts.DACMarket",
                    "Sinodac.Contracts.Delegator"
                };
                List<Transaction> Transactions = new List<Transaction>();
                
                foreach (var key in ContractCodeKeys)
                {
                    var code = ByteString.CopyFrom(GetContractCodes(key));
                    
                    Transactions.Add(await _transactionGeneratingService.GenerateTransactionAsync(
                        ZeroSmartContractAddressNameProvider.Name, nameof(BasicContractZero.DeploySmartContract),
                        new ContractDeploymentInput
                        {
                            Category = KernelConstants.DefaultRunnerCategory,
                            Code = code
                        }.ToByteString()));
                }

                return Transactions;
            }

            return new List<Transaction>();
        }

        private byte[] GetContractCodes(string key)
        {
            return ContractsDeployer.GetContractCodes<DeployContractsSystemTransactionGenerator>(_contractOptions
                .GenesisContractDir)[key];
        }
    }
}