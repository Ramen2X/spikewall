using spikewall.Object;
using System;

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
}
