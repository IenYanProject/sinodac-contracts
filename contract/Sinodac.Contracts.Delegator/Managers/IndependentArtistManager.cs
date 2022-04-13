using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.State;

namespace Sinodac.Contracts.Delegator.Managers
{
    public class IndependentArtistManager
    {
        private readonly CSharpSmartContractContext _context;
        private readonly MappedState<string, IndependentCertificate> _independentCertificateMap;
        private readonly MappedState<string, IndependentArtist> _independentArtistMap;

        public IndependentArtistManager(CSharpSmartContractContext context,
            MappedState<string, IndependentCertificate> independentCertificateMap,
            MappedState<string, IndependentArtist> independentArtistMap)
        {
            _context = context;
            _independentCertificateMap = independentCertificateMap;
            _independentArtistMap = independentArtistMap;
        }

        public void AddIndependentCertificate(IndependentCertificate independentCertificate)
        {
            _independentCertificateMap[independentCertificate.UserName] = independentCertificate;
            _context.Fire(new IndependentCertificateCreated
            {
                FromId = independentCertificate.UserName,
                IndependentCertificate = independentCertificate
            });
        }

        public void AddIndependentArtist(IndependentArtist independentArtist, bool enable, bool isApprove)
        {
            var independentCertificate = _independentCertificateMap[independentArtist.UserName];
            if (independentCertificate == null)
            {
                throw new AssertionException($"用户名为 {independentArtist.UserName} 的个人艺术家未提交认证");
            }

            _independentArtistMap[independentArtist.UserName] = independentArtist;

            if (!isApprove)
            {
                _context.Fire(new IndependentCertificateRejected
                {
                    FromId = independentArtist.Auditor,
                    ArtistUserName = independentArtist.UserName,
                    ArtistName = independentArtist.Name
                });
            }
            else
            {
                _context.Fire(new IndependentCertificateApproved
                {
                    FromId = independentArtist.Auditor,
                    IndependentArtist = independentArtist
                });
                if (enable)
                {
                    _context.Fire(new IndependentArtistEnabled
                    {
                        FromId = independentArtist.Auditor,
                        ArtistUserName = independentArtist.UserName,
                        ArtistName = independentCertificate.Name
                    });
                }
                else
                {
                    _context.Fire(new IndependentArtistDisabled
                    {
                        FromId = independentArtist.Auditor,
                        ArtistUserName = independentArtist.UserName,
                        ArtistName = independentCertificate.Name
                    });
                }
            }
        }

        public void UpdateIndependentCertificate(IndependentCertificate independentCertificate)
        {
            if (_independentCertificateMap[independentCertificate.UserName] == null)
            {
                throw new AssertionException($"个人艺术家 {independentCertificate.UserName} 未曾提交过认证");
            }

            if (!independentCertificate.IsRejected)
            {
                throw new AssertionException("被驳回后才可以更新认证信息");
            }

            independentCertificate.CreateTime = _independentCertificateMap[independentCertificate.UserName].CreateTime;
            _independentCertificateMap[independentCertificate.UserName] = independentCertificate;

            _context.Fire(new IndependentCertificateUpdated
            {
                FromId = independentCertificate.UserName,
                IndependentCertificate = independentCertificate
            });
        }
    }
}