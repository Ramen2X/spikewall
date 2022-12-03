using System.Text.Json.Serialization;

namespace spikewall.Request
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class CommitWheelSpinRequest : BaseRequest
    {
        public long count { get; set; }
    }
}
