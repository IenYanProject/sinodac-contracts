using AElf.Boilerplate.TestBase;
using AElf.Cryptography.ECDSA;
using AElf.Types;
using Sinodac.Contracts.Delegator;

namespace Sinodac.Contracts.DAC
{
    public class DACContractTestBase : DAppContractTestBase<DACContractTestModule>
    {
        internal Address DelegatorContractAddress => GetAddress(DelegatorSmartContractAddressNameProvider.StringName);

        internal DACContractContainer.DACContractStub GetDACContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<DACContractContainer.DACContractStub>(DAppContractAddress, senderKeyPair);
        }

        internal DelegatorContractContainer.DelegatorContractStub GetDelegatorContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<DelegatorContractContainer.DelegatorContractStub>(DelegatorContractAddress, senderKeyPair);
        }
    }
}