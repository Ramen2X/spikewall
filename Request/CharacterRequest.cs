namespace spikewall.Request
{
    public class ChangeCharacterRequest : BaseRequest
    {
        public string? mainCharacterId { get; set; }
        public string? subCharacterId { get; set; }
    }

    public class UpgradeCharacterRequest : BaseRequest
    {
        public string? abilityId { get; set; }
        public string? characterId { get; set; }
    }

    public class UnlockedCharacterRequest : BaseRequest
    {
        public string? characterId { get; set; }
        public string? itemId { get; set; }
    }
}
