using spikewall.Object;

namespace spikewall.Response
{
    public class ChaoResponse : BaseResponse
    {
        public PlayerState playerState { get; set; }
        public Chao[] chaoState { get; set; }
    }

    public class ChangeChaoResponse : BaseResponse
    {
        public PlayerState playerState { get; set; }
    }
}
