using AElf;
using AElf.Types;

namespace Sinodac.Contracts.DAC.Helpers
{
    public static class DACHelper
    {
        public static Hash CalculateDACHash(string dacName, long dacId)
        {
            return HashHelper.ComputeFrom($"{dacName}{dacId}");
        }
    }
}