using Sinodac.Contracts.Delegator;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContract
    {
        private void PerformRegisterPermissions(InitializeInput input)
        {
            State.DelegatorContract.Value = input.DelegatorContractAddress;

            State.DelegatorContract.RegisterSenders.Send(new RegisterSendersInput
            {
                ScopeId = "DAC",
                AddressList = new Delegator.AddressList
                {
                    Value = { input.AdminAddress }
                }
            });
            State.DelegatorContract.RegisterMethods.Send(new RegisterMethodsInput
            {
                ScopeId = "DAC",
                MethodNameList = new StringList
                {
                    Value =
                    {
                        nameof(Create)
                    }
                }
            });
        }

        private void AssertSenderIsDelegatorContract()
        {
            State.DelegatorContract.ForwardCheck.Send(Context.OriginTransactionId);
        }

        // ReSharper disable once InconsistentNaming
        private void AssertSenderIsDACMarketContract()
        {
            Assert(Context.Sender == State.DACMarketContractAddress.Value, "只能通过DAC市场合约铸造新的DAC");
        }
    }
}