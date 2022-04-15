using AElf.Boilerplate.TestBase;
using AElf.ContractTestKit;
using AElf.Cryptography.ECDSA;
using AElf.Types;
using Sinodac.Contracts.DACMarket;
using Sinodac.Contracts.Delegator;

namespace Sinodac.Contracts.DAC
{
    public class DACContractTestBase : DAppContractTestBase<DACContractTestModule>
    {
        internal Address DelegatorContractAddress => GetAddress(DelegatorSmartContractAddressNameProvider.StringName);
        internal Address DACMarketContractAddress => GetAddress(DACMarketSmartContractAddressNameProvider.StringName);

        internal IBlockTimeProvider BlockTimeProvider { get; set; }

        public DACContractTestBase()
        {
            BlockTimeProvider = GetRequiredService<IBlockTimeProvider>();
        }

        internal DACContractContainer.DACContractStub DACContractStub => GetDACContractStub(DefaultAccount.KeyPair);

        internal DACMarketContractContainer.DACMarketContractStub DACMarketContractStub =>
            GetDACMarketContractStub(DefaultAccount.KeyPair);

        internal DACContractContainer.DACContractStub GetDACContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<DACContractContainer.DACContractStub>(DAppContractAddress, senderKeyPair);
        }

        internal DelegatorContractContainer.DelegatorContractStub GetDelegatorContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<DelegatorContractContainer.DelegatorContractStub>(DelegatorContractAddress, senderKeyPair);
        }

        internal DACMarketContractContainer.DACMarketContractStub GetDACMarketContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<DACMarketContractContainer.DACMarketContractStub>(DACMarketContractAddress, senderKeyPair);
        }
    }
}