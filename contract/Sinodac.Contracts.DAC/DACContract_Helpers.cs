using System.Collections.Generic;
using AElf;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using AElf.Types;
using Sinodac.Contracts.Delegator;

namespace Sinodac.Contracts.DAC
{
    public partial class DACContract
    {
        private void PerformRegisterPermissions(InitializeInput input)
        {
            State.DelegatorContract.Value = input.DelegatorContractAddress;

            State.DelegatorContract.RegisterSenders.Send(new RegisterSendersInput
            {
                ScopeId = "DAC",
                AddressList = new Delegator.AddressList
                {
                    Value = { input.AdminAddress }
                }
            });
            State.DelegatorContract.RegisterMethods.Send(new RegisterMethodsInput
            {
                ScopeId = "DAC",
                MethodNameList = new StringList
                {
                    Value =
                    {
                        nameof(Create)
                    }
                }
            });
        }

        private void AssertSenderIsDelegatorContract()
        {
            State.DelegatorContract.ForwardCheck.Send(Context.OriginTransactionId);
        }

        // ReSharper disable once InconsistentNaming
        private void AssertSenderIsDACMarketContract()
        {
            Assert(Context.Sender == State.DACMarketContractAddress.Value, "只能通过DAC市场合约铸造新的DAC");
        }

        private DACTypes InitialDACTypeNameMap()
        {
            if (State.DACTypes.Value != null)
            {
                return State.DACTypes.Value;
            }

            var nftTypes = new DACTypes();
            nftTypes.Value.Add("TP", "图片");
            nftTypes.Value.Add("SP", "视频");
            nftTypes.Value.Add("YP", "音频");
            nftTypes.Value.Add("MX", "3D模型");
            nftTypes.Value.Add("MH", "盲盒");
            State.DACTypes.Value = nftTypes;

            foreach (var pair in nftTypes.Value)
            {
                State.NFTTypeShortNameMap[pair.Value] = pair.Key;
                State.NFTTypeFullNameMap[pair.Key] = pair.Value;
            }

            return nftTypes;
        }

        private void AssertMetadataKeysAreCorrect(IEnumerable<string> metadataKeys)
        {
            var reservedMetadataKey = GetDacMetadataReservedKeys();
            foreach (var metadataKey in metadataKeys)
            {
                Assert(!reservedMetadataKey.Contains(metadataKey), $"Metadata的键 {metadataKey} 已被预留");
            }
        }

        private List<string> GetDacMetadataReservedKeys()
        {
            return new List<string>
            {
                DacTypeMetadataKey,
                AssembledDacsKey,
                AssembledCreditsKey,
            };
        }

        private void PerformMint(MintInput input)
        {
            var dacCollectionInfo = State.DACCollectionInfoMap[input.DacName];
            if (dacCollectionInfo == null)
            {
                throw new AssertionException($"{input.DacName} 还没有创建");
            }

            if (State.OwnDACListMap[input.DacName][input.Owner] == null)
            {
                State.OwnDACListMap[input.DacName][input.Owner] = new Int64List();
            }

            for (var dacId = input.FromDacId; dacId < input.FromDacId.Add(input.Quantity); dacId++)
            {
                var dacHash = CalculateDACHash(input.DacName, input.FromDacId);
                // Mint this DAC
                var newDac = new DACInfo
                {
                    DacName = input.DacName,
                    Creator = dacCollectionInfo.Creator,
                    Price = input.ActualPrice,
                    DacShape = dacCollectionInfo.DacShape,
                    DacType = dacCollectionInfo.DacType,
                    FileId = input.FileId,
                    Metadata = input.Metadata
                };
                State.DACInfoMap[input.DacName][dacId] = newDac;
                State.OwnDACListMap[input.DacName][input.Owner].Value.Add(dacId);
                State.BalanceMap[dacHash][input.Owner] = State.BalanceMap[dacHash][input.Owner].Add(1);
            }
        }

        private Hash CalculateDACHash(string dacName, long dacId)
        {
            return HashHelper.ComputeFrom($"{dacName}{dacId}");
        }
    }
}