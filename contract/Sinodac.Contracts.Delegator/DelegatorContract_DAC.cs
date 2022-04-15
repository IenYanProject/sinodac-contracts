using Google.Protobuf.WellKnownTypes;
using Sinodac.Contracts.DAC;
using Sinodac.Contracts.DACMarket;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty CreateDAC(CreateDACInput input)
        {
            AssertPermission(input.FromId, DAC.Create);
            State.DACContract.Create.Send(new CreateInput
            {
                CreatorUserId = input.FromId,
                CreatorId = input.CreatorId,
                DacName = input.DacName,
                Circulation = input.Circulation,
                Price = input.Price,
                DacShape = input.DacShape,
                DacType = input.DacType,
                ReserveForLottery = input.ReserveForLottery
            });
            if (!string.IsNullOrEmpty(input.SeriesName))
            {
                State.DACMarketContract.AddProtocol.Send(new AddProtocolInput
                {
                    SeriesName = input.SeriesName,
                    DacName = input.DacName
                });
            }

            State.TemporaryTxIdMap[Context.TransactionId] = 2;

            return new Empty();
        }

        public override Empty CreateSeries(CreateSeriesInput input)
        {
            AssertPermission(input.FromId, DAC.CreateSeries);
            State.DACMarketContract.CreateSeries.Send(new DACMarket.CreateSeriesInput
            {
                CreatorUserId = input.FromId,
                CreatorId = input.CreatorId,
                SeriesName = input.SeriesName,
                SeriesDescription = input.SeriesDescription
            });
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }

        public override Empty AddProtocolToSeries(AddProtocolToSeriesInput input)
        {
            AssertPermission(input.FromId, DAC.CreateSeries);
            State.DACMarketContract.AddProtocol.Send(new AddProtocolInput
            {
                DacName = input.DacName,
                SeriesName = input.SeriesName
            });
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }

        public override Empty AuditDAC(AuditDACInput input)
        {
            AssertPermission(input.FromId, DAC.AuditDetail);
            if (input.IsApprove)
            {
                State.DACContract.ApproveProtocol.Send(new ApproveProtocolInput
                {
                    DacName = input.DacName,
                    IsApprove = true
                });
            }
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }

        public override Empty ListDAC(ListDACInput input)
        {
            AssertPermission(input.FromId, DAC.List);
            State.DACMarketContract.List.Send(new ListInput
            {
                DacName = input.DacName,
                PublicTime = input.PublicTime
            });
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }

        public override Empty MintDAC(MintDACInput input)
        {
            AssertPermission(input.FromId, DAC.List);
            State.DACContract.Mint.Send(new MintInput
            {
                DacName = input.DacName,
                FromDacId = input.FromDacId,
                Quantity = input.Quantity
            });
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }

        public override Empty DelistDAC(DelistDACInput input)
        {
            AssertPermission(input.FromId, DAC.List);
            State.DACMarketContract.Delist.Send(new DelistInput
            {
                DacName = input.DacName
            });
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }

        public override Empty BindRedeemCode(BindRedeemCodeInput input)
        {
            AssertPermission(input.FromId, DAC.Create);
            State.DACContract.MintForRedeemCode.Send(new MintForRedeemCodeInput
            {
                DacName = input.DacName,
                RedeemCodeHashList = { input.RedeemCodeHashList },
                Skip = input.Skip
            });
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }

        public override Empty Buy(BuyInput input)
        {
            State.DACMarketContract.Buy.Send(new DACMarket.BuyInput
            {
                To = GetVirtualAddress(input.FromId),
                DacName = input.DacName,
                DacId = input.DacId,
                Price = input.Price,
                UserId = input.FromId
            });
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }

        public override Empty Redeem(RedeemInput input)
        {
            State.DACMarketContract.Redeem.Send(new DACMarket.RedeemInput
            {
                To = GetVirtualAddress(input.FromId),
                RedeemCode = input.RedeemCode,
                UserId = input.FromId
            });
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }

        public override Empty Box(BoxInput input)
        {
            AssertPermission(input.FromId, DAC.Create);
            State.DACMarketContract.Box.Send(new DACMarket.BoxInput
            {
                DacName = input.DacName
            });
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }

        public override Empty Unbox(UnboxInput input)
        {
            State.DACMarketContract.Unbox.Send(new DACMarket.UnboxInput
            {
                DacName = input.DacName,
                BoxId = input.BoxId,
                To = GetVirtualAddress(input.FromId),
                UserId = input.FromId
            });
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }

        public override Empty Give(GiveInput input)
        {
            var from = GetVirtualAddress(input.FromId);
            Assert(
                State.DACContract.IsOwner.Call(new IsOwnerInput
                    { DacName = input.DacName, DacId = input.DacId, Owner = from }).Value,
                $"转出方不持有DAC {input.DacName}-{input.DacId}");
            State.DACContract.TransferFrom.Send(new TransferFromInput
            {
                DacName = input.DacName,
                DacId = input.DacId,
                From = from,
                To = GetVirtualAddress(input.ToId)
            });
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }
    }
}