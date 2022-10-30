using spikewall.Object;
using System;

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
}
