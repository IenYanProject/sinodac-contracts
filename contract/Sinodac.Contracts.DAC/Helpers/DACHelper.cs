using AElf;
using AElf.Types;
using Volo.Abp.Timing;

namespace Sinodac.Contracts.DAC.Helpers
{
    public static class DACHelper
    {
        private static readonly IClock _clock;
        
        
        public static Hash CalculateDACHash(string dacName, long dacId)
        {
            var date = _clock.Now;
            return HashHelper.ComputeFrom($"{dacName}{dacId}-{date}");
        }
    }
}