using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Credit
{
    public partial class CreditContract : CreditContractContainer.CreditContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            State.CreateTokenWhiteListMap[input.AdminAddress] = true;
            State.CreateTokenWhiteListMap[input.DacContractAddress] = true;
            State.Admin.Value = input.AdminAddress;
            return new Empty();
        }

        public override Empty Create(CreateInput input)
        {
            if (Context.Origin != Context.Sender)
            {
                Assert(IsAddressInCreateTokenWhiteList(Context.Sender), "No permission to create new credit kind.");
            }
            
            

            return new Empty();
        }
    }
}