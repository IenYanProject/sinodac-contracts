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
                CollectionList = new StringList()
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

            AssertProtocolExists(input.DacName);

            var series = State.DACSeriesMap[input.SeriesName];
            if (series == null)
            {
                throw new AssertionException($"系列藏品 {input.SeriesName} 尚未创建");
            }

            var protocol = State.DACContract.GetDACProtocolInfo.Call(new StringValue { Value = input.DacName });
            Assert(!string.IsNullOrEmpty(protocol.DacName), $"藏品 {input.DacName} 尚未创建");

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
            Assert(State.DACContract.IsDACProtocolApproved.Call(new StringValue{Value = input.DacName}).Value, $"DAC {input.DacName} 还没有通过审核");
            var publicTime = input.PublicTime ?? Context.CurrentBlockTime;
            AssertProtocolExists(input.DacName);
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
            AssertProtocolExists(input.DacName);
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
                CopyrightId = input.CopyrightId
            };
            State.DACCopyrightMap[input.DacName] = dacCopyright;
            if (input.IsConfirm)
            {
                Context.Fire(new CopyrightConfirmed
                {
                    DacName = dacCopyright.DacName,
                    CopyrightId = dacCopyright.CopyrightId,
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
            Assert(State.PublicTimeMap[input.DacName] != null, $"{input.DacName} 没有设置上架时间");
            Assert(State.PublicTimeMap[input.DacName] <= Context.CurrentBlockTime, $"{input.DacName} 还没有上架");

            var isMysteryBox = protocol.Circulation == protocol.ReserveForLottery;
            if (isMysteryBox)
            {
                var boxId = CalculateBoxId(input.DacName, input.DacId);
                State.BoxInfoMap[boxId] = new BoxInfo
                {
                    DacName = input.DacName,
                    DacId = input.DacId,
                    Price = input.Price
                };
                if (State.OwnBoxIdListMap[input.To] == null)
                {
                    State.OwnBoxIdListMap[input.To] = new StringList
                    {
                        Value = { boxId }
                    };
                }
                else
                {
                    State.OwnBoxIdListMap[input.To].Value.Add(boxId);
                }

                Context.Fire(new BoxSold
                {
                    UserAddress = input.To,
                    UserId = input.UserId,
                    DacName = input.DacName,
                    DacId = input.DacId,
                    BoxId = boxId,
                    Price = input.Price
                });
            }
            else
            {
                State.DACContract.InitialTransfer.Send(new InitialTransferInput
                {
                    DacName = input.DacName,
                    DacId = input.DacId,
                    To = input.To
                });

                Context.Fire(new DACSold
                {
                    UserAddress = input.To,
                    UserId = input.UserId,
                    DacId = input.DacId,
                    DacName = input.DacName,
                    Price = input.Price
                });
            }

            return new Empty();
        }

        private string CalculateBoxId(string dacName, long dacId)
        {
            if (State.BoxIdSeedMap[dacName] == null)
            {
                throw new AssertionException("盲盒还未打包");
            }

            var seed = State.BoxIdSeedMap[dacName];
            return HashHelper
                .ComputeFrom($"{seed} - {dacId} - {Context.OriginTransactionId.ToHex()} - {Context.CurrentBlockTime}")
                .ToHex();
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

            Context.Fire(new CodeRedeemed
            {
                DacName = dacInfo.DacName,
                DacId = dacInfo.DacId,
                RedeemCode = input.RedeemCode,
                UserAddress = input.To,
                UserId = input.UserId
            });
            return new Empty();
        }

        public override Empty Box(BoxInput input)
        {
            AssertSenderIsDelegatorContract();
            SetBoxIdSeed(input.DacName);
            Context.Fire(new Boxed
            {
                DacName = input.DacName
            });
            return new Empty();
        }

        private void SetBoxIdSeed(string dacName)
        {
            State.BoxIdSeedMap[dacName] = Context.OriginTransactionId.ToHex();
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

            var ownBoxIdList = State.OwnBoxIdListMap[input.To];
            if (ownBoxIdList == null)
            {
                throw new AssertionException($"用户 {input.To} 名下没有盲盒");
            }

            Assert(ownBoxIdList.Value.Contains(input.BoxId), $"编号为 {input.BoxId} 的盲盒不属于用户 {input.To}");
            ownBoxIdList.Value.Remove(input.BoxId);

            State.DACContract.InitialTransfer.Send(new InitialTransferInput
            {
                DacName = boxInfo.DacName,
                DacId = boxInfo.DacId,
                To = input.To
            });

            Context.Fire(new Unboxed
            {
                UserId = input.UserId,
                UserAddress = input.To,
                DacName = boxInfo.DacName,
                DacId = boxInfo.DacId,
                BoxId = input.BoxId
            });
            return new Empty();
        }
    }
}