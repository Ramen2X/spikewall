using System.Text.Json.Serialization;

namespace spikewall.Request
{
    public class ChangeCharacterRequest : BaseRequest
    {
        public int mainCharacterId { get; set; }
        public int subCharacterId { get; set; }
    }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class UpgradeCharacterRequest : BaseRequest
    {
        public int abilityId { get; set; }
        public int characterId { get; set; }
    }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class UnlockedCharacterRequest : BaseRequest
    {
        public int characterId { get; set; }
        public int itemId { get; set; }
    }
}
