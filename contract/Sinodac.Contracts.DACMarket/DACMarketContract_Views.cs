using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.DACMarket
{
    public partial class DACMarketContract
    {
        public override DACSeries GetDACSeries(StringValue input)
        {
            return State.DACSeriesMap[input.Value];
        }

        public override DACCopyright GetDACCopyright(StringValue input)
        {
            return State.DACCopyrightMap[input.Value];
        }

        public override Timestamp GetPublicTime(StringValue input)
        {
            return State.PublicTimeMap[input.Value];
        }

        public override BoxInfo GetBoxInfo(StringValue input)
        {
            return State.BoxInfoMap[input.Value];
        }

        public override StringList GetOwnBoxIdList(Address input)
        {
            return State.OwnBoxIdListMap[input];
        }
    }
}