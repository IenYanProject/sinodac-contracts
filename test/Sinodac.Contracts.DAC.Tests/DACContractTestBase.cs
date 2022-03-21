using AElf.Boilerplate.TestBase;
using AElf.Cryptography.ECDSA;

namespace Sinodac.Contracts.DAC
{
    public class DACContractTestBase : DAppContractTestBase<DACContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address DAppContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);

        internal DACContractContainer.DACContractStub GetDACContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<DACContractContainer.DACContractStub>(DAppContractAddress, senderKeyPair);
        }
    }
}