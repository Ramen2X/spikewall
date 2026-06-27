using System.Text.Json.Serialization;

namespace spikewall.Request
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class EquipChaoRequest : BaseRequest
    {
        public int mainChaoId { get; set; }
        public int subChaoId { get; set; }
    }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class CommitChaoWheelSpinRequest : BaseRequest
    {
        public int count { get; set; }
    }


}
