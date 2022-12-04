using spikewall.Object;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing roulette information
    /// </summary>
    public class WheelOptionsResponse : BaseResponse
    {
        public WheelOptions wheelOptions { get; set; }

        public WheelOptionsResponse(WheelOptions wheelOptions)
        {
            this.wheelOptions = wheelOptions;
        }

        public WheelOptionsResponse()
        {
            this.wheelOptions = new WheelOptions();
        }
    }

    public class CommitWheelSpinResponse : BaseResponse
    {
        public PlayerState playerState { get; set; }

        // FIXME: ChaoState is also sent here!!

        public WheelOptions wheelOptions { get; set; }
    }
}
