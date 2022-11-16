using System.Text.Json.Serialization;

namespace spikewall.Request
{
    public class ChangeCharacterRequest : BaseRequest
    {
        public string mainCharacterId { get; set; }
        public string subCharacterId { get; set; }
    }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class UpgradeCharacterRequest : BaseRequest
    {
        public int abilityId { get; set; }
        public string characterId { get; set; }
    }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class UnlockedCharacterRequest : BaseRequest
    {
        public string characterId { get; set; }
        public int itemId { get; set; }
    }
}
