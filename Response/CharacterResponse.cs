using spikewall.Object;

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

    public class ChangeCharacterResponse : BaseResponse
    {
        public PlayerState playerState { get; set; }
    }
}
