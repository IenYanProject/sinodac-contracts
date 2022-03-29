using System.Linq;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;
using Sinodac.Contracts.DAC;
using Sinodac.Contracts.Delegator;

namespace Sinodac.Contracts.DACMarket
{
    public partial class DACMarketContract
    {
        private void AssertSenderIsDelegatorContract()
        {
            State.DelegatorContract.ForwardCheck.Send(Context.OriginTransactionId);
        }

        private DACCollectionInfo AssertPermission(string fromId, string dacName)
        {
            var dacCollection = State.DACContract.GetDACCollectionInfo.Call(new StringValue { Value = dacName });
            if (dacCollection == null)
            {
                throw new AssertionException($"单件藏品 {dacName} 尚未创建");
            }

            if (dacCollection.IsCreatedByOrganization)
            {
                var organizationName = dacCollection.CreatorName;
                var organizationUnit = State.DelegatorContract.GetOrganizationUnitList.Call(
                    new GetOrganizationUnitListInput
                    {
                        OrganizationName = organizationName
                    }).Value.FirstOrDefault();
                if (organizationUnit == null)
                {
                    throw new AssertionException($"机构 {organizationName} 不存在");
                }

                Assert(organizationUnit.AdminList.Value.Contains(fromId),
                    $"没有权限操作单件藏品 {dacName}");
            }
            else
            {
                Assert(dacCollection.CreatorName == fromId,
                    $"没有权限操作单件藏品 {dacName}");
            }

            return dacCollection;
        }
    }
}