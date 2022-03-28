using System;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Credit
{
    public partial class CreditContract : CreditContractContainer.CreditContractBase
    {
        public override Empty Initialize(InitializeInput input)
        {
            State.CreateCreditWhiteListMap[input.AdminAddress] = true;
            State.CreateCreditWhiteListMap[input.DacContractAddress] = true;
            State.Admin.Value = input.AdminAddress;
            State.DelegatorContract.Value = input.DelegatorContractAddress;
            PerformRegisterPermissions(input);
            return new Empty();
        }

        public override Empty Create(CreateInput input)
        {
            Assert(IsAddressInCreateCreditWhiteList(Context.Sender), "无权创建积分类型");
            Assert(State.CreditInfoMap[input.Symbol] != null, $"积分类型 {input.Symbol} 已经存在了");

            var creditInfo = new CreditInfo
            {
                Symbol = input.Symbol,
                TotalSupply = input.TotalSupply,
                Decimals = input.Decimals,
                Issuer = input.Issuer,
                IsBurnable = input.IsBurnable,
                Usage = input.Usage
            };
            State.CreditInfoMap[input.Symbol] = creditInfo;

            Context.Fire(new CreditCreated
            {
                Symbol = input.Symbol,
                CreditInfo = creditInfo
            });

            return new Empty();
        }

        public override Empty Issue(IssueInput input)
        {
            Assert(input.To != null, "未指定积分接收地址");
            var creditInfo = State.CreditInfoMap[input.Symbol];
            if (creditInfo == null)
            {
                throw new AssertionException($"积分类型 {input.Symbol} 不存在");
            }

            Assert(creditInfo.Issuer == Context.Sender, "无权发放积分");
            creditInfo.Issued = creditInfo.Issued.Add(input.Amount);
            creditInfo.Supply = creditInfo.Supply.Add(input.Amount);

            Assert(creditInfo.Issued <= creditInfo.TotalSupply, "积分发行数量超过限度");
            State.CreditInfoMap[input.Symbol] = creditInfo;
            ModifyBalance(input.To, input.Symbol, input.Amount);
            Context.Fire(new Issued
            {
                Symbol = input.Symbol,
                Amount = input.Amount,
                To = input.To,
                Memo = input.Memo
            });
            return new Empty();
        }

        public override Empty Burn(BurnInput input)
        {
            var creditInfo = AssertValidCredit(input.Symbol, input.Amount);
            Assert(creditInfo.IsBurnable, "该积分类型被设定为无法销毁");
            ModifyBalance(Context.Sender, input.Symbol, -input.Amount);
            creditInfo.Supply = creditInfo.Supply.Sub(input.Amount);
            Context.Fire(new Burned
            {
                Burner = Context.Sender,
                Symbol = input.Symbol,
                Amount = input.Amount
            });
            return new Empty();
        }

        public override Empty Transfer(TransferInput input)
        {
            AssertValidCredit(input.Symbol, input.Amount);
            DoTransfer(Context.Sender, input.To, input.Symbol, input.Amount, input.Memo);
            DealWithExternalInfoDuringTransfer(new TransferFromInput
            {
                From = Context.Sender,
                To = input.To,
                Amount = input.Amount,
                Symbol = input.Symbol,
                Memo = input.Memo
            });
            return new Empty();
        }

        public override Empty TransferFrom(TransferFromInput input)
        {
            AssertValidCredit(input.Symbol, input.Amount);
            var allowance = State.AllowanceMap[input.From][Context.Sender][input.Symbol];
            if (allowance < input.Amount)
            {
                throw new AssertionException(
                    $"{input.From} 对 {Context.Sender} 的积分 {input.Symbol} 的授权额度不足，当前额度为 {allowance}，试图转走 {input.Amount}");
            }

            DoTransfer(input.From, input.To, input.Symbol, input.Amount, input.Memo);
            DealWithExternalInfoDuringTransfer(input);
            State.AllowanceMap[input.From][Context.Sender][input.Symbol] = allowance.Sub(input.Amount);
            return new Empty();
        }

        public override Empty Approve(ApproveInput input)
        {
            AssertValidCredit(input.Symbol, input.Amount);
            State.AllowanceMap[Context.Sender][input.Spender][input.Symbol] = input.Amount;
            Context.Fire(new Approved
            {
                Owner = Context.Sender,
                Spender = input.Spender,
                Symbol = input.Symbol,
                Amount = input.Amount
            });
            return new Empty();
        }

        public override Empty UnApprove(UnApproveInput input)
        {
            AssertValidCredit(input.Symbol, input.Amount);
            var oldAllowance = State.AllowanceMap[Context.Sender][input.Spender][input.Symbol];
            var amountOrAll = Math.Min(input.Amount, oldAllowance);
            State.AllowanceMap[Context.Sender][input.Spender][input.Symbol] = oldAllowance.Sub(amountOrAll);
            Context.Fire(new UnApproved
            {
                Owner = Context.Sender,
                Spender = input.Spender,
                Symbol = input.Symbol,
                Amount = amountOrAll
            });
            return new Empty();
        }
    }
}