using spikewall.Object;
using System;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing player-specific information about the game chao
    /// </summary>
    public class ChaoStateResponse : BaseResponse
    {
        public Chao[] chaoState { get; set; }

        public ChaoStateResponse(Chao[] chaoState)
        {
            this.chaoState = chaoState;
        }
    }
}
