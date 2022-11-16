using spikewall.Object;
using System;

namespace spikewall.Response
{
    /// <summary>
    /// Response used to craft the response for many character
    /// operations, like unlocking a character or upgrading one.
    /// </summary>
    public class CharacterResponse : BaseResponse
    {
        public PlayerState playerState { get; set; }
        public Character[] characterState { get; set; }
    }
}