using System.Text.Json.Serialization;

namespace spikewall.Request
{
    /// <summary>
    /// Class to store data from a login request.
    /// </summary>
    public class LoginRequest : BaseRequest
    {
        public string? device { get; set; }
        public string? platform { get; set; }
        public string? language { get; set; }
        public string? salesLocate { get; set; }
        public string? storeId { get; set; }
        public string? platform_sns { get; set; }
        public LineAuth? lineAuth { get; set; }
    }
}
