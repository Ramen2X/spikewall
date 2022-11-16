using spikewall.Object;
using System;

namespace spikewall.Response
{
    public class UpgradeCharacterResponse : BaseResponse
    {
        public PlayerState playerState { get; set; }
        public Character[] characterState { get; set; }
    }
}