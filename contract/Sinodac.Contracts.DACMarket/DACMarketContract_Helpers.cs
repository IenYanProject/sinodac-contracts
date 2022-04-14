using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.DACMarket
{
    public partial class DACMarketContract
    {
        private void AssertSenderIsDelegatorContract()
        {
            State.DelegatorContract.ForwardCheck.Send(Context.OriginTransactionId);
        }

        private void AssertProtocolExists(string dacName)
        {
            var protocol = State.DACContract.GetDACProtocolInfo.Call(new StringValue { Value = dacName });
            if (protocol == null)
            {
                throw new AssertionException($"单件藏品 {dacName} 尚未创建");
            }
        }
    }
}