using spikewall.Object;

namespace spikewall.Response
{
    public class ChaoWheelSpinResponse : BaseResponse
    {
        public PlayerState playerState {  get; set; }
        public Character[] characterState { get; set; }
        public Chao[] chaoState { get; set; }
        public ChaoWheelOptions ChaoWheelOptions { get; set; }
        public ChaoSpinResult ChaoSpinResults { get; set; }
    }
}
