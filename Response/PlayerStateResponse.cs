using spikewall.Object;

namespace spikewall.Response
{
    public class PlayerStateResponse : BaseResponse
    {
        public PlayerState playerState { get; set; }

        public PlayerStateResponse(PlayerState playerState)
        {
            this.playerState = playerState;
        }
    }
}
