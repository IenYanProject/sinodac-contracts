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
            AssertPermission(input.FromId, Profile.CertificateIndependentArtist);
            var independentCertificate = new IndependentCertificate
            {
                CreateTime = Context.CurrentBlockTime,
                Description = input.Description,
                Email = input.Email,
                Id = input.Id,
                Location = input.Location,
                Name = input.Name,
                PhoneNumber = input.PhoneNumber,
                PhotoIds = { input.PhotoIds },
                UserName = input.FromId
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
        /// 通过或驳回个人艺术家认证
        /// 需要权限Permission:IndependentArtist:Create（管理员角色）
        /// 创建和存储IndependentArtist实例，key为个人艺术家的账户名（不是姓名，防止重复）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Empty CreateIndependentArtist(CreateIndependentArtistInput input)
        {
            AssertPermission(input.FromId, Permission.IndependentArtist.Create);
            var independentCertificate = State.IndependentCertificateMap[input.ArtistUserName];
            if (independentCertificate == null)
            {
                throw new AssertionException($"用户名为 {input.ArtistUserName} 的个人艺术家未提交认证");
            }

            var independentArtist = new IndependentArtist
            {
                Name = input.ArtistUserName,
                CreateTime = Context.CurrentBlockTime,
                UserName = independentCertificate.Name,
                IsRejected = !input.IsApprove,
                Enabled = input.Enable
            };

            State.IndependentArtistMap[input.ArtistUserName] = independentArtist;

            if (!input.IsApprove)
            {
                Context.Fire(new IndependentCertificateRejected
                {
                    FromId = input.FromId,
                    ArtistUserName = input.ArtistUserName,
                    ArtistName = independentCertificate.Name
                });
            }
            else
            {
                Context.Fire(new IndependentCertificateApproved
                {
                    FromId = input.FromId,
                    IndependentArtist = independentArtist
                });
                if (input.Enable)
                {
                    Context.Fire(new IndependentArtistEnabled
                    {
                        FromId = input.FromId,
                        ArtistUserName = independentCertificate.UserName,
                        ArtistName = independentCertificate.Name
                    });
                }
                else
                {
                    Context.Fire(new IndependentArtistDisabled
                    {
                        FromId = input.FromId,
                        ArtistUserName = independentCertificate.UserName,
                        ArtistName = independentCertificate.Name
                    });
                }
            }

            return new Empty();
        }

        /// <summary>
        /// 当个人艺术家认证被驳回后，通过这个方法修改认证信息
        /// 需要权限Permission:IndependentArtist:Create（管理员角色）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="AssertionException"></exception>
        public override Empty UpdateIndependentCertificate(UpdateIndependentCertificateInput input)
        {
            AssertPermission(input.FromId, Profile.CertificateIndependentArtist);
            var independentCertificate = State.IndependentCertificateMap[input.FromId];
            if (independentCertificate == null)
            {
                throw new AssertionException($"个人艺术家 {input.Name} 未曾提交过认证");
            }

            Assert(independentCertificate.IsRejected, "被驳回后才可以更新认证信息");

            State.IndependentCertificateMap[input.FromId] = new IndependentCertificate
            {
                CreateTime = independentCertificate.CreateTime,
                Description = input.Description,
                Email = input.Email,
                Id = input.Id,
                Location = input.Location,
                Name = input.Name,
                PhoneNumber = input.PhoneNumber,
                PhotoIds = { input.PhotoIds },
                UserName = input.FromId,
                LatestEditTime = Context.CurrentBlockTime
            };

            Context.Fire(new IndependentCertificateUpdated
            {
                FromId = input.FromId,
                IndependentCertificate = State.IndependentCertificateMap[input.FromId]
            });
            return new Empty();
        }

        /// <summary>
        /// 管理员修改个人艺术家认证信息
        /// 需要权限Permission:IndependentArtist:Update（管理员角色）
        /// 其实目前而言只能改改艺术家名字ArtistName
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Empty UpdateIndependentArtist(UpdateIndependentArtistInput input)
        {
            AssertPermission(input.FromId, Permission.IndependentArtist.Update);
            var independentArtist = State.IndependentArtistMap[input.ArtistUserName];
            if (independentArtist == null)
            {
                throw new AssertionException($"账号 {input.ArtistUserName} 未认证个人艺术家或未审核");
            }

            Assert(!independentArtist.IsRejected, $"账号 {input.ArtistUserName} 的个人艺术家认证被驳回");

            State.IndependentArtistMap[input.ArtistUserName] = new IndependentArtist
            {
                UserName = input.ArtistUserName,
                Name = input.ArtistName,
                LatestEditTime = Context.CurrentBlockTime,
                // Stay old values.
                CreateTime = independentArtist.CreateTime
            };
            return new Empty();
        }
    }
}