using spikewall.Object;
using System;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing information about the Chao prize wheel roulette
    /// </summary>
    public class PrizeChaoWheelSpinResponse : BaseResponse
    {
        public string[]? prizeList { get; set; }

        public PrizeChaoWheelSpinResponse()
        {
            this.prizeList = new string[0];
        }
    }
}
