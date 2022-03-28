using AElf.Sdk.CSharp;
using Google.Protobuf.WellKnownTypes;

namespace Sinodac.Contracts.Delegator
{
    public partial class DelegatorContract
    {
        /// <summary>
        /// 提交个人艺术家认证
        /// 需要权限：Profile:Certificate:IndependentArtist（默认角色）
        /// 创建和存储IndependentCertificate实例，key为个人艺术家的账户名（userName）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Empty CreateIndependentCertificate(CreateIndependentCertificateInput input)
        {
            AssertPermission(input.FromId, false, Profile.CertificateIndependentArtist);
            var independentCertificate = new IndependentCertificate
            {
                CreateTime = Context.CurrentBlockTime,
                Description = input.Description,
                Email = input.Email,
                Id = input.Id,
                Location = input.Location,
                Name = input.Name,
                OrganizationName = input.OrganizationName,
                PhoneNumber = input.PhoneNumber,
                PhotoIds = { input.PhotoIds },
                UserName = input.UserName
            };
            State.IndependentCertificateMap[input.FromId] = independentCertificate;
            Context.Fire(new IndependentCertificateCreated
            {
                FromId = input.FromId,
                IndependentCertificate = independentCertificate
            });
            return new Empty();
        }

        /// <summary>
        /// 通过个人艺术家认证
        /// 需要权限Permission:IndependentArtist:Create（管理员角色）
        /// 创建和存储IndependentArtist实例，key为个人艺术家的账户名
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Empty CreateIndependentArtist(CreateIndependentArtistInput input)
        {
            AssertPermission(input.FromId, false, Permission.IndependentArtist.Create);
            var independentCertificate = State.IndependentCertificateMap[input.ArtistUserName];
            if (independentCertificate == null)
            {
                throw new AssertionException($"用户名为 {input.ArtistUserName} 的个人艺术家未提交认证");
            }

            if (!input.IsApprove)
            {
                Context.Fire(new IndependentCertificateRejected
                {
                    FromId = input.FromId,
                    ArtistUserName = input.ArtistUserName,
                    ArtistName = independentCertificate.Name
                });
                return new Empty();
            }

            var independentArtist = new IndependentArtist
            {
                Name = input.ArtistUserName,
                CreateTime = Context.CurrentBlockTime,
                Enabled = input.Enable,
                UserName = independentCertificate.Name
            };
            Context.Fire(new IndependentCertificateApproved
            {
                FromId = input.FromId,
                IndependentArtist = independentArtist
            });
            return new Empty();
        }

        public override Empty UpdateIndependentCertificate(UpdateIndependentCertificateInput input)
        {
            AssertPermission(input.FromId, false, Profile.CertificateOrganizationUnit);
            throw new AssertionException("暂不支持");
        }

        public override Empty UpdateIndependentArtist(UpdateIndependentArtistInput input)
        {
            return base.UpdateIndependentArtist(input);
        }

        public override Empty DisableIndependentArtist(DisableIndependentArtistInput input)
        {
            return base.DisableIndependentArtist(input);
        }
    }
}