using System.Text.Json.Serialization;

namespace spikewall.Request
{
    /// <summary>
    /// Class from which requests are derived from.
    /// </summary>
    public class BaseRequest
    {
        public string? sessionId { get; set; }
        public string? version { get; set; }

        // This is a custom client addition, ignore if not present
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? revivalVerId { get; set; }
        public string? seq { get; set; }
    }
}
