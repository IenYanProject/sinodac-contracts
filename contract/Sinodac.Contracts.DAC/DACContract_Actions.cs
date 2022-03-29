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
            if (input.Metadata != null)
            {
                AssertMetadataKeysAreCorrect(input.Metadata.Value.Keys);
            }

            var dacCollection = new DACCollectionInfo
            {
                DacName = input.DacName,
                Creator = input.Creator ?? Context.Sender,
                Metadata = input.Metadata,
                Circulation = input.Circulation,
                DacShape = input.DacShape,
                DacType = input.DacType,
                DescriptionFileId = input.DescriptionFileId,
                IsCreatedByOrganization = input.IsCreatedByOrganization,
                Price = input.Price,
                IsReal = input.IsReal,
                RealCirculation = input.RealCirculation,
                PurchaseLimit = input.PurchaseLimit
            };
            State.DACCollectionInfoMap[input.DacName] = dacCollection;

            Context.Fire(new DACCollectionCreated
            {
                DacName = input.DacName,
                DacCollectionInfo = dacCollection
            });
            return new Empty();
        }

        public override Empty Mint(MintInput input)
        {
            AssertSenderIsDACMarketContract();
            if (input.Metadata != null && input.Metadata.Value.Any())
            {
                AssertMetadataKeysAreCorrect(input.Metadata.Value.Keys);
            }

            PerformMint(input);
            return new Empty();
        }

        public override Empty Burn(BurnInput input)
        {
            return new Empty();
        }

        public override Empty Transfer(TransferInput input)
        {
            return new Empty();
        }

        public override Empty TransferFrom(TransferFromInput input)
        {
            return new Empty();
        }

        public override Empty Approve(ApproveInput input)
        {
            return new Empty();
        }

        public override Empty UnApprove(UnApproveInput input)
        {
            return new Empty();
        }

        public override Empty Recast(RecastInput input)
        {
            return new Empty();
        }

        public override Hash Assemble(AssembleInput input)
        {
            return base.Assemble(input);
        }

        public override Empty Disassemble(DisassembleInput input)
        {
            return new Empty();
        }

        public override Empty AddDACType(AddDACTypeInput input)
        {
            return new Empty();
        }

        public override Empty RemoveDACType(StringValue input)
        {
            return new Empty();
        }

        public override Empty ApproveCollection(ApproveCollectionInput input)
        {
            return new Empty();
        }
    }
}