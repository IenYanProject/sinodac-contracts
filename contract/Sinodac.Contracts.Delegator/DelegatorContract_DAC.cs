using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Sinodac.Contracts.DAC;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        public override Empty MintDAC(MintDACInput input)
        {
            //TODO 权限
            var mintInput = new MintInput()
            {
                DacName = input.DacName,
                FromDacId = input.FromDacId,
                Quantity = input.Quantity,
                DacFile=input.DacFile
            };
            State.DACContract.Mint.Send(mintInput);
            State.TemporaryTxIdMap[Context.TransactionId] = 1;
            return new Empty();
        }

        public override Empty Buy(BuyInput input)
        {
            State.DACContract.InitialTransfer.Send(new InitialTransferInput
            {
                To = Context.Sender,
                File = input.File,
                NftInfoId = input.NftInfoid,
                NftHash = input.NftHash,
                Owner = input.FromId,
                NftId = input.NftId,
                NftName = input.NftName
            });
            State.TemporaryTxIdMap[Context.TransactionId] = 1;

            return new Empty();
        }

        public override NftInfoList GetNftInfoList(Empty input)
        {
            var nftInfos = State.DACContract.GetNFTInfoList.Call(Context.Sender).NftInfo.ToList();

            var result = new NftInfoList();

            foreach (var item in nftInfos)
            {
                result.NftInfo.Add(new NftInfo()
                {
                    NftFile = item.DacFile,
                    NftHash = item.DacHash,
                    NftId = item.DacId,
                    NftName = item.DacName 
                });
            }
            
            return result;
        }

    }
}