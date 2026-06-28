using spikewall.Object;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing information about the Chao prize wheel roulette
    /// </summary>
    public class PrizeChaoWheelSpinResponse : BaseResponse
    {
        public Chao[]? prizeList { get; set; }

        public PrizeChaoWheelSpinResponse(Chao[] prizeList)
        {
            this.prizeList = prizeList;
        }
    }
}
