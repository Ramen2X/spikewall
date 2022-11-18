using spikewall.Object;
using System.Text.Json.Serialization;

namespace spikewall.Response
{
    /// <summary>
    /// Response for when a new user is created for the client
    /// </summary>
    public class NewUserResponse : BaseResponse
    {
        public string userId { get; set; }
        public string password { get; set; }
        public string key { get; set; }
        public string countryId { get; set; }
        public string countryCode { get; set; }

        public NewUserResponse(string userId, string password, string key, string countryId, string countryCode)
        {
            this.userId = userId;
            this.password = password;
            this.key = key;
            this.countryId = countryId;
            this.countryCode = countryCode;

            this.statusCode = SRStatusCode.PassWordError;
        }
    }

    /// <summary>
    /// Response for client checking the key
    /// </summary>
    public class ServerKeyCheckResponse : BaseResponse
    {
        public string? key { get; set; }

        public ServerKeyCheckResponse(string key)
        {
            this.key = key;

            this.statusCode = SRStatusCode.PassWordError;
        }
    }

    /// <summary>
    /// Response for when an existing user returns
    /// </summary>
    public class LoginResponse : BaseResponse
    {
        public string? userName { get; set; }
        public string? sessionId { get; set; }
        public long? sessionTimeLimit { get; set; }
        public long? energyRecveryTime { get; set; }
        public long? energyRecoveryMax { get; set; }

        public spikewall.Object.Item? inviteBasicIncentiv { get; set; }

        public LoginResponse()
        {
            this.inviteBasicIncentiv = new Item();
        }

        public LoginResponse(string userName, string sessionId, long sessionTimeLimit, long energyRecveryTime, long energyRecoveryMax, long itemId, long numItem)
        {
            this.userName = userName;
            this.sessionId = sessionId;
            this.sessionTimeLimit = sessionTimeLimit;
            this.energyRecveryTime = energyRecveryTime;
            this.energyRecoveryMax = energyRecoveryMax;

            this.inviteBasicIncentiv = new Item();
            this.inviteBasicIncentiv.itemId = itemId;
            this.inviteBasicIncentiv.numItem = numItem;
        }
    }

    public class VariousParameterResponse : BaseResponse
    {
        public long? cmSkipCount { get; set; }
        public long? energyRecoveryMax { get; set; }
        public long? energyRecveryTime { get; set; }
        public long? onePlayCmCount { get; set; }
        public long? onePlayContinueCount { get; set; }
        public long? isPurchased { get; set; }

        // TODO: Get these defaults from the config
        public VariousParameterResponse()
        {
            cmSkipCount = 5;
            energyRecoveryMax = 5;
            energyRecveryTime = 660; // 11 minutes
            onePlayCmCount = 0;
            onePlayContinueCount = 5;
            isPurchased = 0;
        }
    }

    public class LoginInformationResponse : BaseResponse
    {
        public Information[] informations { get; set; }
        public OperatorInformation[] operatorEachInfos { get; set; }
        public sbyte numOperatorInfo { get; set; }
    }

    public class LoginGetTickerResponse : BaseResponse
    {
        public Ticker[]? tickerList { get; set; }
    }

    public class LoginBonusResponse : BaseResponse
    {
        public LoginBonusStatus? loginBonusStatus { get; set; }
        public LoginBonusReward[]? loginBonusRewardList { get; set; }
        public LoginBonusReward[]? firstLoginBonusRewardList { get; set; }
        public long? startTime { get; set; }
        public long? endTime { get; set; }
        public long? rewardId { get; set; }
        public long? rewardDays { get; set; }
        public long? firstRewardDays { get; set; }

        public LoginBonusResponse()
        {
            // TODO: Stub
            loginBonusStatus = new LoginBonusStatus();
            loginBonusRewardList = Array.Empty<LoginBonusReward>();
            firstLoginBonusRewardList = Array.Empty<LoginBonusReward>();
            startTime = 0;
            endTime = -4;
            rewardId = -1;
            rewardDays = -1;
            firstRewardDays = -1;
        }
    }

    public class LoginBonusSelectResponse : BaseResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Item[]? rewardList { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Item[]? firstRewardList { get; set; }
    }
}
