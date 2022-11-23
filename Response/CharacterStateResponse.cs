using spikewall.Object;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing player-specific information about the game characters
    /// </summary>
    public class CharacterStateResponse : BaseResponse
    {
        public Character[] characterState { get; set; }

        public CharacterStateResponse(Character[] characterState)
        {
            this.characterState = characterState;
        }
    }
}
