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
            return HashHelper.ComputeFrom($"{dacName}{dacId}");
        }
        public static Hash CalculateDACHash(string dacName, long dacId,int dacTs)
        {
            return HashHelper.ComputeFrom($"{dacName}{dacId}-{dacTs}");
        }
    }
}