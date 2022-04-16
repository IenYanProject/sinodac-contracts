using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AElf;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Sinodac.Contracts.Delegator;
using Xunit;
using TimestampHelper = AElf.Kernel.TimestampHelper;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContractTests
    {
        [Fact(DisplayName = "【DAC】创建藏品系列")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> CreateSeriesTest()
        {
            var stub = await InitializeAsync();

            await stub.CreateSeries.SendAsync(new CreateSeriesInput
            {
                SeriesName = "新版十二生肖",
                SeriesDescription = "二创",
                CreatorId = "国家博物馆",
                FromId = "国博馆员1"
            });

            var series = await DACMarketContractStub.GetDACSeries.CallAsync(new StringValue
            {
                Value = "新版十二生肖"
            });
            series.CreatorId.ShouldBe("国家博物馆");
            series.CreatorUserId.ShouldBe("国博馆员1");
            series.CollectionCount.ShouldBe(0);

            return stub;
        }

        [Fact(DisplayName = "【DAC】为系列添加不存在的藏品")]
        internal async Task AddProtocolTest()
        {
            var stub = await CreateSeriesTest();
            var executionResult = await stub.AddProtocolToSeries.SendWithExceptionAsync(new AddProtocolToSeriesInput
            {
                SeriesName = "新版十二生肖",
                DacName = "老鼠人"
            });
            executionResult.TransactionResult.Error.ShouldContain("尚未创建");
        }

        [Fact(DisplayName = "【DAC】创建单件DAC，同时加入藏品系列")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> CreateDACTest()
        {
            var stub = await CreateSeriesTest();

            await stub.CreateDAC.SendAsync(new CreateDACInput
            {
                DacName = "老鼠人",
                SeriesName = "新版十二生肖",
                Price = 999,
                Circulation = 10000,
                CreatorId = "国家博物馆",
                DacShape = "长方形",
                DacType = "图片",
                FromId = "国博馆员2",
                ReserveForLottery = 100
            });

            var protocol = await DACContractStub.GetDACProtocolInfo.CallAsync(new StringValue
            {
                Value = "老鼠人"
            });
            protocol.Circulation.ShouldBe(10000);
            protocol.ReserveFrom.ShouldBePositive();
            protocol.ReserveFrom.ShouldBeLessThan(10000 - 100);

            var series = await DACMarketContractStub.GetDACSeries.CallAsync(new StringValue
            {
                Value = "新版十二生肖"
            });
            series.CollectionCount.ShouldBe(1);
            series.CollectionList.Value.First().ShouldBe("老鼠人");

            return stub;
        }

        [Fact(DisplayName = "【DAC】还没有审核通过DAC就试图上架")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> ListWithoutApprovalTest()
        {
            var stub = await CreateDACTest();
            var executionResult = await stub.ListDAC.SendWithExceptionAsync(new ListDACInput
            {
                DacName = "老鼠人",
                FromId = "国博馆员3",
                PublicTime = TimestampHelper.GetUtcNow(),
            });
            executionResult.TransactionResult.Error.ShouldContain("没有通过审核");
            return stub;
        }

        [Fact(DisplayName = "【DAC】审核通过DAC")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> ApproveProtocolTest()
        {
            var stub = await CreateDACTest();
            await stub.AuditDAC.SendAsync(new AuditDACInput
            {
                DacName = "老鼠人",
                FromId = "审核员1号",
                IsApprove = true
            });
            var isApproved = await DACContractStub.IsDACProtocolApproved.CallAsync(new StringValue
            {
                Value = "老鼠人"
            });
            isApproved.Value.ShouldBeTrue();

            return stub;
        }

        [Fact(DisplayName = "【DAC】上架DAC")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> ListTest()
        {
            var stub = await ApproveProtocolTest();
            await stub.ListDAC.SendAsync(new ListDACInput
            {
                DacName = "老鼠人",
                FromId = "国博馆员3",
                PublicTime = TimestampHelper.GetUtcNow().AddSeconds(1)
            });

            var publicTime = await DACMarketContractStub.GetPublicTime.CallAsync(new StringValue { Value = "老鼠人" });
            publicTime.ShouldNotBeNull();

            Thread.Sleep(2000);

            return stub;
        }

        [Fact(DisplayName = "【DAC】上架后需要批量mint出来")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> BatchMintTest()
        {
            var stub = await ListTest();
            await stub.MintDAC.SendAsync(new MintDACInput
            {
                DacName = "老鼠人",
                FromId = "管理员",
                FromDacId = 1,
                Quantity = 10000
            });

            {
                var isMinted = await DACContractStub.IsMinted.CallAsync(new IsMintedInput
                {
                    DacName = "老鼠人",
                    DacId = 1
                });
                isMinted.Value.ShouldBeTrue();
            }

            {
                var isMinted = await DACContractStub.IsMinted.CallAsync(new IsMintedInput
                {
                    DacName = "老鼠人",
                    DacId = 10000
                });
                isMinted.Value.ShouldBeTrue();
            }
            return stub;
        }

        [Fact(DisplayName = "【DAC】兑换码还没有完成绑定就购买DAC")]
        internal async Task BuyWithoutBindRedeemCodeTest()
        {
            var stub = await BatchMintTest();
            var executionResult = await stub.Buy.SendWithExceptionAsync(new BuyInput
            {
                DacName = "老鼠人",
                DacId = 1111,
                FromId = "张三",
                Price = 999
            });

            executionResult.TransactionResult.Error.ShouldContain("兑换码还没有完成绑定");
        }

        private List<string> RedeemCodeList { get; set; }

        [Fact(DisplayName = "【DAC】绑定兑换码")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> BindRedeemCodeTest()
        {
            var stub = await BatchMintTest();
            RedeemCodeList = Enumerable.Range(1, 120).Select(i => Guid.NewGuid().ToString()).ToList();
            var redeemCodeHashList = RedeemCodeList.Select(HashHelper.ComputeFrom).ToList();
            var protocol = await DACContractStub.GetDACProtocolInfo.CallAsync(new StringValue { Value = "老鼠人" });
            await stub.BindRedeemCode.SendAsync(new BindRedeemCodeInput
            {
                DacName = "老鼠人",
                FromId = "管理员",
                RedeemCodeHashList = { redeemCodeHashList.Take(30) },
                FromDacId = protocol.ReserveFrom
            });
            
            var executionResult = await stub.BindRedeemCode.SendWithExceptionAsync(new BindRedeemCodeInput
            {
                DacName = "老鼠人",
                FromId = "管理员",
                RedeemCodeHashList = { redeemCodeHashList.Skip(30) },
                FromDacId = protocol.ReserveFrom.Add(30)
            });
            executionResult.TransactionResult.Error.ShouldContain("抽奖码给多了");

            await stub.BindRedeemCode.SendAsync(new BindRedeemCodeInput
            {
                DacName = "老鼠人",
                FromId = "管理员",
                RedeemCodeHashList = { redeemCodeHashList.Skip(30).Take(30) },
                FromDacId = protocol.ReserveFrom.Add(30)
            });
            
            await stub.BindRedeemCode.SendAsync(new BindRedeemCodeInput
            {
                DacName = "老鼠人",
                FromId = "管理员",
                RedeemCodeHashList = { redeemCodeHashList.Skip(60).Take(30) },
                FromDacId = protocol.ReserveFrom.Add(60)
            });

            await stub.BindRedeemCode.SendAsync(new BindRedeemCodeInput
            {
                DacName = "老鼠人",
                FromId = "管理员",
                RedeemCodeHashList = { redeemCodeHashList.Skip(90).Take(10) },
                FromDacId = protocol.ReserveFrom.Add(90)
            });

            var isBindCompleted = await DACContractStub.IsBindCompleted.CallAsync(new StringValue { Value = "老鼠人" });
            isBindCompleted.Value.ShouldBeTrue();

            return stub;
        }

        [Fact(DisplayName = "【DAC】购买DAC")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> BuyTest()
        {
            var stub = await BindRedeemCodeTest();
            BlockTimeProvider.SetBlockTime(TimestampHelper.GetUtcNow().AddDays(2));
            await stub.Buy.SendAsync(new BuyInput
            {
                DacName = "老鼠人",
                DacId = 1,
                FromId = "张三",
                Price = 999
            });

            var address = await stub.CalculateUserAddress.CallAsync(new StringValue { Value = "张三" });
            var balance = await DACContractStub.GetBalance.CallAsync(new GetBalanceInput
            {
                DacName = "老鼠人",
                Owner = address
            });
            balance.Balance.ShouldBe(1);
            
            var isOwner = await DACContractStub.IsOwner.CallAsync(new IsOwnerInput
            {
                DacName = "老鼠人",
                DacId = 100,
                Owner = address
            });
            isOwner.Value.ShouldBeTrue();
            return stub;
        }

        [Fact(DisplayName = "【DAC】使用兑换码兑换DAC")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> RedeemTest()
        {
            var stub = await BindRedeemCodeTest();
            await stub.Redeem.SendAsync(new RedeemInput
            {
                RedeemCode = RedeemCodeList[10],
                FromId = "李四"
            });

            var address = await stub.CalculateUserAddress.CallAsync(new StringValue { Value = "李四" });
            var balance = await DACContractStub.GetBalance.CallAsync(new GetBalanceInput
            {
                DacName = "老鼠人",
                Owner = address
            });
            balance.Balance.ShouldBe(1);
            
            var isOwner = await DACContractStub.IsOwner.CallAsync(new IsOwnerInput
            {
                DacName = "老鼠人",
                DacId = 100,
                Owner = address
            });
            isOwner.Value.ShouldBeTrue();
            return stub;
        }

        [Fact(DisplayName = "【DAC】创建盲盒")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> CreateMysteryBoxTest()
        {
            var stub = await InitializeAsync();
            await stub.CreateDAC.SendAsync(new CreateDACInput
            {
                DacName = "小聋人",
                Price = 888,
                Circulation = 5000,
                CreatorId = "国家博物馆",
                DacShape = "长方形",
                DacType = "图片",
                FromId = "国博馆员2",
                ReserveForLottery = 5000
            });

            var protocol = await DACContractStub.GetDACProtocolInfo.CallAsync(new StringValue
            {
                Value = "小聋人"
            });
            protocol.Circulation.ShouldBe(5000);
            protocol.ReserveForLottery.ShouldBe(5000);
            protocol.ReserveFrom.ShouldBe(1);
            return stub;
        }

        [Fact(DisplayName = "【DAC】审核、打包和上架盲盒")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> BoxTest()
        {
            var stub = await CreateMysteryBoxTest();
            
            await stub.AuditDAC.SendAsync(new AuditDACInput
            {
                FromId = "管理员",
                DacName = "小聋人",
                IsApprove = true
            });

            await stub.Box.SendAsync(new BoxInput
            {
                FromId = "管理员",
                DacName = "小聋人"
            });

            await stub.ListDAC.SendAsync(new ListDACInput
            {
                FromId = "管理员",
                DacName = "小聋人",
                PublicTime = TimestampHelper.GetUtcNow().AddSeconds(1)
            });

            await stub.MintDAC.SendAsync(new MintDACInput
            {
                FromId = "管理员",
                DacName = "小聋人",
                FromDacId = 1,
                Quantity = 5000
            });

            Thread.Sleep(2000);

            return stub;
        }

        [Fact(DisplayName = "【DAC】购买盲盒")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> BuyBoxTest()
        {
            var stub = await BoxTest();
            await stub.Buy.SendAsync(new BuyInput
            {
                FromId = "王五",
                DacName = "小聋人",
                DacId = 100,
                Price = 777
            });

            var address = await stub.CalculateUserAddress.CallAsync(new StringValue { Value = "王五" });
            var ownBoxIdList = await DACMarketContractStub.GetOwnBoxIdList.CallAsync(address);
            ownBoxIdList.Value.Count.ShouldBe(1);

            var boxInfo =
                await DACMarketContractStub.GetBoxInfo.CallAsync(new StringValue
                    { Value = ownBoxIdList.Value.First() });
            boxInfo.DacName.ShouldBe("小聋人");
            boxInfo.DacId.ShouldBe(100);
            boxInfo.Price.ShouldBe(777);
            return stub;
        }

        [Fact(DisplayName = "【DAC】打开盲盒")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> UnboxTest()
        {
            var stub = await BuyBoxTest();
            var address = await stub.CalculateUserAddress.CallAsync(new StringValue { Value = "王五" });
            var ownBoxIdList = await DACMarketContractStub.GetOwnBoxIdList.CallAsync(address);
            await stub.Unbox.SendAsync(new UnboxInput
            {
                DacName = "小聋人",
                FromId = "王五",
                BoxId = ownBoxIdList.Value.First()
            });
            
            var balance = await DACContractStub.GetBalance.CallAsync(new GetBalanceInput
            {
                DacName = "小聋人",
                Owner = address
            });
            balance.Balance.ShouldBe(1);

            var isOwner = await DACContractStub.IsOwner.CallAsync(new IsOwnerInput
            {
                DacName = "小聋人",
                DacId = 100,
                Owner = address
            });
            isOwner.Value.ShouldBeTrue();
            return stub;
        }

        [Fact(DisplayName = "【DAC】下架DAC")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> DelistTest()
        {
            var stub = await ListTest();
            await stub.DelistDAC.SendAsync(new DelistDACInput
            {
                DacName = "老鼠人",
                FromId = "管理员"
            });

            var publicTime = await DACMarketContractStub.GetPublicTime.CallAsync(new StringValue
            {
                Value = "老鼠人"
            });
            publicTime.Nanos.ShouldBe(0);
            return stub;
        }
    }
}