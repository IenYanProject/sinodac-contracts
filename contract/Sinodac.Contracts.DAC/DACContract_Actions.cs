using System;
using System.Linq;
using AElf.CSharp.Core;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContract
    {
        public override Empty Create(CreateInput input)
        {
            AssertSenderIsDelegatorContract();
            var dacService = GetDACService();
            var reserveFrom = input.ReserveFrom == 0
                ? PickReserveFrom(input.Circulation, input.ReserveForLottery)
                : input.ReserveFrom;
            dacService.CreateProtocol(new DACProtocolInfo
            {
                CreatorUserId = input.CreatorUserId,
                DacName = input.DacName,
                Price = input.Price,
                CreatorId = input.CreatorId,
                Circulation = input.Circulation,
                DacShape = input.DacShape,
                DacType = input.DacType,
                ReserveFrom = reserveFrom,
                ReserveForLottery = input.ReserveForLottery
            });
            return new Empty();
        }

        private long PickReserveFrom(long circulation, long reserveForLottery)
        {
            var randomNumber = Math.Abs(Context.GetRandomHash(Context.TransactionId).ToInt64());
            var reserveFrom = (randomNumber % circulation).Add(1);
            if (reserveFrom.Add(reserveForLottery) > circulation)
            {
                return Math.Abs(reserveFrom.Sub(reserveForLottery));
            }

            return reserveFrom;
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
            AssertSenderIsDelegatorContract();
            var dacService = GetDACService();
            dacService.Transfer(input.DacName, input.DacId, input.From, input.To);
            return new Empty();
        }

        public override Empty ApproveProtocol(ApproveProtocolInput input)
        {
            AssertSenderIsDelegatorContract();
            if (input.IsApprove)
            {
                var dacService = GetDACService();
                dacService.ApproveProtocol(input.DacName);
            }

            return new Empty();
        }

        public override Empty MintForRedeemCode(MintForRedeemCodeInput input)
        {
            AssertSenderIsDelegatorContract();
            var dacService = GetDACService();
            dacService.BindRedeemCode(input.DacName, input.RedeemCodeHashList.ToList(), (int)input.Skip);
            return new Empty();
        }
    }
}