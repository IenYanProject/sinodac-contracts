using System;
using System.Linq;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using Sinodac.Contracts.DAC;
using Sinodac.Contracts.Delegator;

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
                Creator = input.Creator,
                CreateTime = Context.CurrentBlockTime,
                CreatorOrganization = input.CreatorOrganization,
                SeriesDescription = input.SeriesDescription,
                CoverFileId = input.CoverFileId,
                SeriesName = input.SeriesName
            };
            State.DACSeriesMap[input.SeriesName] = series;
            Context.Fire(new SeriesCreated
            {
                SeriesName = series.SeriesName,
                Creator = series.Creator,
                CreatorOrganization = series.CreatorOrganization,
                DacSeries = series
            });
            return new Empty();
        }

        public override Empty AddCollection(AddCollectionInput input)
        {
            if (Context.Sender != State.DACContract.Value)
            {
                AssertSenderIsDelegatorContract();
            }

            AssertPermission(input.FromId, input.DacName);

            var series = State.DACSeriesMap[input.SeriesName];
            if (series == null)
            {
                throw new AssertionException($"系列藏品 {input.SeriesName} 尚未创建");
            }

            series.CollectionList.Value.Add(input.DacName);
            series.CollectionCount = series.CollectionCount.Add(1);
            State.DACSeriesMap[input.SeriesName] = series;
            Context.Fire(new CollectionAdded
            {
                FromId = input.FromId,
                DacName = input.DacName,
                SeriesName = input.SeriesName
            });
            return new Empty();
        }

        public override Empty List(ListInput input)
        {
            AssertSenderIsDelegatorContract();
            var publicTime = input.PublicTime ?? Context.CurrentBlockTime;
            var dacCollection = AssertPermission(input.FromId, input.DacName);
            Assert(dacCollection.FileInfoList.Count > input.UseFileInfoIndex, "数组越界");
            State.ListInfoMap[input.DacName] = new ListInfo
            {
                PublicTime = input.PublicTime,
                UseFileId = dacCollection.FileInfoList[input.UseFileInfoIndex].FileId
            };
            Context.Fire(new DACListed
            {
                DacName = input.DacName,
                FromId = input.FromId,
                PublicTime = publicTime
            });
            return new Empty();
        }

        public override Empty Delist(DelistInput input)
        {
            AssertSenderIsDelegatorContract();
            Assert(State.ListInfoMap[input.DacName] != null, $"{input.DacName} 还没有上架");
            AssertPermission(input.FromId, input.DacName);
            State.ListInfoMap.Remove(input.DacName);
            Context.Fire(new DACDelisted
            {
                DacName = input.DacName,
                FromId = input.FromId
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
            var dacCollection = State.DACContract.GetDACCollectionInfo.Call(new StringValue
            {
                Value = input.DacName
            });
            if (string.IsNullOrEmpty(dacCollection.DacName))
            {
                throw new AssertionException($"查无此DAC：{input.DacName}");
            }

            var quantity = Math.Min(input.Quantity, dacCollection.PurchaseLimit);
            var fromDacId = dacCollection.Minted.Add(1);

            Assert(State.ListInfoMap[input.DacName] != null, $"{input.DacName} 还没有上架");

            State.DACContract.Mint.Send(new MintInput
            {
                DacName = input.DacName,
                ActualPrice = input.ActualPrice,
                Owner = input.ReceiverAddress,
                FileId = State.ListInfoMap[input.DacName].UseFileId,
                FromDacId = fromDacId,
                Quantity = quantity
            });

            return new Empty();
        }
    }
}