using System.Linq;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContract
    {
        public override Empty Create(CreateInput input)
        {
            AssertSenderIsDelegatorContract();
            var dacService = GetDACService();
            dacService.CreateProtocol(new DACProtocolInfo
            {
                DacName = input.DacName,
                Price = input.Price,
                CreatorId = input.CreatorId,
                Circulation = input.Circulation,
                DacShape = input.DacShape,
                DacType = input.DacType,
                DescriptionFileId = input.DescriptionFileId,
            });
            return new Empty();
        }

        public override Empty Mint(MintInput input)
        {
            AssertSenderIsDACMarketContract();
            var dacService = GetDACService();
            dacService.BatchMint(input.DacName, input.FromDacId, input.Quantity);
            return new Empty();
        }

        public override Empty InitialTransfer(InitialTransferInput input)
        {
            AssertSenderIsDACMarketContract();
            var dacService = GetDACService();
            dacService.InitialTransfer(input.DacName, input.DacId, input.To);
            return new Empty();
        }

        public override Empty Transfer(TransferInput input)
        {
            var dacService = GetDACService();
            dacService.Transfer(input.DacName, input.DacId, Context.Sender, input.To);
            return new Empty();
        }

        public override Empty TransferFrom(TransferFromInput input)
        {
            AssertSenderIsDACMarketContract();
            var dacService = GetDACService();
            dacService.Transfer(input.DacName, input.DacId, input.From, input.To);
            return new Empty();
        }

        public override Empty ApproveProtocol(ApproveProtocolInput input)
        {
            AssertSenderIsDelegatorContract();
            if (input.Approve)
            {
                var dacService = GetDACService();
                dacService.ApproveProtocol(input.DacName);
            }
            return new Empty();
        }
    }
}