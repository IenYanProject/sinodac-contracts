using System.Linq;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf;
using Sinodac.Contracts.Delegator;

namespace Sinodac.Contracts.Credit
{
    public partial class CreditContract
    {
        private bool IsAddressInCreateCreditWhiteList(Address address)
        {
            return State.CreateCreditWhiteListMap[address];
        }

        private void PerformRegisterPermissions(InitializeInput input)
        {
            State.DelegatorContract.Value = input.DelegatorContractAddress;

            State.DelegatorContract.RegisterSenders.Send(new RegisterSendersInput
            {
                ScopeId = "Credit",
                AddressList = new AddressList
                {
                    Value = { input.AdminAddress }
                }
            });
            State.DelegatorContract.RegisterMethods.Send(new RegisterMethodsInput
            {
                ScopeId = "Credit",
                MethodNameList = new StringList
                {
                    Value =
                    {
                        nameof(Create),
                        nameof(Issue),
                        nameof(Transfer),
                        nameof(TransferFrom),
                        nameof(Approve),
                        nameof(UnApprove)
                    }
                }
            });
        }

        private void ModifyBalance(Address address, string symbol, long addAmount)
        {
            var before = GetBalance(address, symbol);
            if (addAmount < 0 && before < -addAmount)
            {
                throw new AssertionException(
                    $"地址 {address} 的 {symbol} 积分不足 （{before}），仍需 {-addAmount}");
            }

            var target = before.Add(addAmount);
            State.BalanceMap[address][symbol] = target;
        }

        private long GetBalance(Address address, string symbol)
        {
            return State.BalanceMap[address][symbol];
        }

        private void DoTransfer(Address from, Address to, string symbol, long amount, string memo = null)
        {
            Assert(from != to, "积分转出和转入方不可以是同一地址");
            ModifyBalance(from, symbol, -amount);
            ModifyBalance(to, symbol, amount);
            Context.Fire(new Transferred
            {
                From = from,
                To = to,
                Symbol = symbol,
                Amount = amount,
                Memo = memo ?? string.Empty
            });
        }

        private CreditInfo AssertValidCredit(string symbol, long amount)
        {
            AssertValidSymbolAndAmount(symbol, amount);
            var creditInfo = State.CreditInfoMap[symbol];
            Assert(creditInfo != null, $"未找到积分类型 {symbol}");
            return creditInfo;
        }

        private void AssertValidSymbolAndAmount(string symbol, long amount)
        {
            Assert(!string.IsNullOrEmpty(symbol), "积分类型为空");
            Assert(amount > 0, "积分数量不为正");
        }

        private void DealWithExternalInfoDuringTransfer(TransferFromInput input)
        {
            var creditInfo = State.CreditInfoMap[input.Symbol];
            if (creditInfo.ExternalInfo == null) return;
            if (creditInfo.ExternalInfo.Value.ContainsKey(TransferCallbackExternalInfoKey))
            {
                var callbackInfo =
                    JsonParser.Default.Parse<CallbackInfo>(
                        creditInfo.ExternalInfo.Value[TransferCallbackExternalInfoKey]);
                Context.SendInline(callbackInfo.ContractAddress, callbackInfo.MethodName, input);
            }

            FireExternalLogEvent(creditInfo, input);
        }

        private void FireExternalLogEvent(CreditInfo creditInfo, TransferFromInput input)
        {
            if (creditInfo.ExternalInfo.Value.ContainsKey(LogEventExternalInfoKey))
            {
                Context.FireLogEvent(new LogEvent
                {
                    Name = creditInfo.ExternalInfo.Value[LogEventExternalInfoKey],
                    Address = Context.Self,
                    NonIndexed = input.ToByteString()
                });
            }
        }
    }
}