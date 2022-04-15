using System.Linq;
using System.Threading.Tasks;
using AElf.ContractTestKit;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Sinodac.Contracts.Delegator;
using Xunit;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContractTests
    {
        [Fact(DisplayName = "创建藏品系列")]
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

        [Fact(DisplayName = "为系列添加不存在的藏品")]
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

        [Fact(DisplayName = "创建单件DAC，同时加入藏品系列")]
        internal async Task<DelegatorContractContainer.DelegatorContractStub> CreateDAC()
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
            
            var series = await DACMarketContractStub.GetDACSeries.CallAsync(new StringValue
            {
                Value = "新版十二生肖"
            });
            series.CollectionCount.ShouldBe(1);
            series.CollectionList.Value.First().ShouldBe("老鼠人");

            return stub;
        }
    }
}