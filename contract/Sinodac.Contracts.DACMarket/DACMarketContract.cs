using System.Linq;
using AElf;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;
using Sinodac.Contracts.DAC;

namespace Sinodac.Contracts.DACMarket
{
    public partial class DACMarketContract : DACMarketContractContainer.DACMarketContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            State.DelegatorContract.Value = input.DelegatorContractAddress;
            State.DACContract.Value = input.DacContractAddress;
            return new Empty();
        }

        public override Empty CreateSeries(CreateSeriesInput input)
        {
            AssertSenderIsDelegatorContract();
            var series = new DACSeries
            {
                CreatorUserId = input.CreatorUserId,
                CreatorId = input.CreatorId,
                SeriesDescription = input.SeriesDescription,
                SeriesName = input.SeriesName,
                CreateTime = Context.CurrentBlockTime,
            };
            State.DACSeriesMap[input.SeriesName] = series;
            Context.Fire(new SeriesCreated
            {
                SeriesName = series.SeriesName,
                CreatorUserId = series.CreatorUserId,
                CreatorId = series.CreatorId,
                DacSeries = series
            });
            return new Empty();
        }

        public override Empty AddProtocol(AddProtocolInput input)
        {
            if (Context.Sender != State.DACContract.Value)
            {
                AssertSenderIsDelegatorContract();
            }

            AssertDACExists(input.DacName);

            var series = State.DACSeriesMap[input.SeriesName];
            if (series == null)
            {
                throw new AssertionException($"系列藏品 {input.SeriesName} 尚未创建");
            }

            series.CollectionList.Value.Add(input.DacName);
            series.CollectionCount = series.CollectionCount.Add(1);
            State.DACSeriesMap[input.SeriesName] = series;
            Context.Fire(new ProtocolAdded
            {
                DacName = input.DacName,
                SeriesName = input.SeriesName
            });
            return new Empty();
        }

        public override Empty List(ListInput input)
        {
            AssertSenderIsDelegatorContract();
            var publicTime = input.PublicTime ?? Context.CurrentBlockTime;
            AssertDACExists(input.DacName);
            State.PublicTimeMap[input.DacName] = publicTime;
            Context.Fire(new DACListed
            {
                DacName = input.DacName,
                PublicTime = publicTime
            });
            return new Empty();
        }

        public override Empty Delist(DelistInput input)
        {
            AssertSenderIsDelegatorContract();
            Assert(State.PublicTimeMap[input.DacName] != null, $"{input.DacName} 还没有上架");
            AssertDACExists(input.DacName);
            State.PublicTimeMap.Remove(input.DacName);
            Context.Fire(new DACDelisted
            {
                DacName = input.DacName,
            });
            return new Empty();
        }

        public override Empty ConfirmCopyright(ConfirmCopyrightInput input)
        {
            AssertSenderIsDelegatorContract();
            var dacCopyright = new DACCopyright
            {
                DacName = input.DacName,
                IsConfirmed = input.IsConfirm,
                FromId = input.FromId,
                CopyrightId = input.CopyrightId
            };
            if (input.IsConfirm)
            {
                Context.Fire(new CopyrightConfirmed
                {
                    DacName = dacCopyright.DacName,
                    CopyrightId = dacCopyright.CopyrightId,
                    FromId = dacCopyright.FromId
                });
            }

            return new Empty();
        }

        public override Empty Buy(BuyInput input)
        {
            AssertSenderIsDelegatorContract();

            var protocol = State.DACContract.GetDACProtocolInfo.Call(new StringValue
            {
                Value = input.DacName
            });
            Assert(!string.IsNullOrEmpty(protocol.DacName), $"查无此DAC：{input.DacName}");
            Assert(input.To.Value.Any(), "无效的DAC的接受地址");
            Assert(State.PublicTimeMap[input.DacName] != null, $"{input.DacName} 还没有上架");

            var isMysteryBox = protocol.Circulation == protocol.ReserveForLottery;
            if (isMysteryBox)
            {
                var boxId = CalculateBoxId(input.DacName, input.DacId);
                State.BoxInfoMap[boxId] = new BoxInfo
                {
                    DacName = input.DacName,
                    DacId = input.DacId
                };
                if (State.OwnBoxCodeMap[input.To] == null)
                {
                    State.OwnBoxCodeMap[input.To] = new StringList
                    {
                        Value = { boxId }
                    };
                }
                else
                {
                    State.OwnBoxCodeMap[input.To].Value.Add(boxId);
                }
            }
            else
            {
                State.DACContract.InitialTransfer.Send(new InitialTransferInput
                {
                    DacName = input.DacName,
                    DacId = input.DacId,
                    To = input.To
                });
            }

            return new Empty();
        }

        private string CalculateBoxId(string dacName, long dacId)
        {
            var seed = State.BoxIdSeedMap[dacName];
            return HashHelper.ComputeFrom($"{seed} - {dacId} - {Context.OriginTransactionId.ToHex()}").ToHex();
        }

        public override Empty Redeem(RedeemInput input)
        {
            AssertSenderIsDelegatorContract();
            var redeemCodeHash = HashHelper.ComputeFrom(input.RedeemCode);
            var dacInfo = State.DACContract.GetRedeemCodeDAC.Call(redeemCodeHash);
            Assert(dacInfo.DacName != null, $"兑换码 {input.RedeemCode} 无效");
            Assert(State.PublicTimeMap[dacInfo.DacName] != null, $"{dacInfo.DacName} 还没有上架");
            State.DACContract.InitialTransfer.Send(new InitialTransferInput
            {
                DacName = dacInfo.DacName,
                DacId = dacInfo.DacId,
                To = input.To
            });

            return new Empty();
        }

        public override Empty Box(BoxInput input)
        {
            AssertSenderIsDelegatorContract();
            State.BoxIdSeedMap[input.DacName] = Context.OriginTransactionId.ToHex();
            return new Empty();
        }

        public override Empty Unbox(UnboxInput input)
        {
            AssertSenderIsDelegatorContract();
            var boxInfo = State.BoxInfoMap[input.BoxId];
            if (boxInfo == null)
            {
                throw new AssertionException($"盲盒 {input.BoxId} 无效");
            }

            Assert(State.PublicTimeMap[input.DacName] != null, $"{input.DacName} 还没有上架");
            State.DACContract.InitialTransfer.Send(new InitialTransferInput
            {
                DacName = boxInfo.DacName,
                DacId = boxInfo.DacId,
                To = input.To
            });
            return new Empty();
        }
    }
}