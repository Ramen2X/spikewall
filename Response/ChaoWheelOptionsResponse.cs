using spikewall.Object;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing player-specific information about the game chao
    /// </summary>
    public class ChaoWheelOptionsResponse : BaseResponse
    {
        public ChaoWheelOptions chaoWheelOptions { get; set; }

        public ChaoWheelOptionsResponse()
        {
            this.chaoWheelOptions = new ChaoWheelOptions();
        }
    }
}
