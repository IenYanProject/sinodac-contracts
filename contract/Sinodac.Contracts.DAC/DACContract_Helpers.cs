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
                    Value = { input.Admin }
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
    }
}