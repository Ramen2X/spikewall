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

    public class LoginBonusSelectRequest : BaseRequest
    {
        public long? rewardId { get; set; }
        public long? rewardDays { get; set; }
        public long? rewardSelect { get; set; }
        public long? firstRewardDays { get; set; }
        public long? firstRewardSelect { get; set; }
    }
}
