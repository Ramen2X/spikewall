using System.Text.Json.Serialization;

namespace spikewall.Request
{
    /// <summary>
    /// Class to store data from a login request.
    /// </summary>
    [JsonPolymorphic]
    public class LoginRequest : BaseRequest
    {
        public string device;
        public string platform;
        public string language;
        public string salesLocate;
        public string storeId;
        public string platform_sns;
        public LineAuth lineAuth;
    }
}
