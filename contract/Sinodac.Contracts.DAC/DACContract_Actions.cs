using System;
using System.Linq;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContract
    {
        public override Empty Mint(MintInput input)
        {
            AssertSenderIsDelegatorContract();
            var dacService = GetDACService();
            dacService.BatchMint(input.DacName, input.FromDacId, input.DacFile, input.Quantity, input.Circulation);
            return new Empty();
        }

        public override Empty InitialTransfer(InitialTransferInput input)
        {
            AssertSenderIsDelegatorContract();
            var dacService = GetDACService();
            dacService.InitialTransfer(input.NftInfoId, input.To, input.NftHash, input.File, input.Owner, input.NftName, input.NftId);
            return new Empty();
        }

        public override NFTInfoList GetNFTInfoList(Address input)
        {
            AssertSenderIsDelegatorContract();

            var result = State.OwnerNFTMap[input];
            
            return result;
        }
    }
}