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
        public Int64? sessionTimeLimit { get; set; }
        public Int64? energyRecveryTime { get; set; }
        public Int64? energyRecoveryMax { get; set; }

        public class InviteBasicIncentive
        {
            public Int64? itemId { get; set; }
            public Int64? numItem { get; set; }
        }

        public InviteBasicIncentive? inviteBasicIncentiv { get; set; }

        public LoginResponse()
        {
            this.inviteBasicIncentiv = new InviteBasicIncentive();

            this.statusCode = 0;
        }

        public LoginResponse(string userName, string sessionId, Int64 sessionTimeLimit, Int64 energyRecveryTime, Int64 energyRecoveryMax, Int64 itemId, Int64 numItem)
        {
            this.userName = userName;
            this.sessionId = sessionId;
            this.sessionTimeLimit = sessionTimeLimit;
            this.energyRecveryTime = energyRecveryTime;
            this.energyRecoveryMax = energyRecoveryMax;

            this.inviteBasicIncentiv = new InviteBasicIncentive();
            this.inviteBasicIncentiv.itemId = itemId;
            this.inviteBasicIncentiv.numItem = numItem;
        }
    }
}
