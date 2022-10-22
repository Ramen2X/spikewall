namespace spikewall.Request
{
    /// <summary>
    /// Class from which requests are derived from.
    /// </summary>
    public class BaseRequest
    {
        public string? sessionId { get; set; }
        public string? version { get; set; }
        public string? revivalVerId { get; set; }
        public string? seq { get; set; }
    }
}
