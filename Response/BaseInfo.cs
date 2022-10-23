namespace spikewall.Response
{
    public class BaseInfo
    {
        public enum SRStatusCode
        {
            /// <summary>No error has occurred.</summary>
            Ok = 0,
            /// <summary>Security error has occurred.</summary>
            ServerSecurityError = -19001,
            /// <summary>Server version and client version don't match.</summary>
            VersionDifference = -19002,
            /// <summary>Server failed to decrypt client data (likely due to a key mismatch).</summary>
            DecryptionFailure = -19003,
            /// <summary>Client's key was not what was expected.</summary>
            ParamHashDifference = -19004,
            /// <summary>Server is in maintenance mode in preparation for a new client version.</summary>
            ServerNextVersion = -19990, // show next version maintenance window
            /// <summary>Server is undergoing maintenance.</summary>
            ServerMaintenance = -19997,
            /// <summary>Server is too busy to handle requests at this time.</summary>
            ServerBusyError = -19998,
            /// <summary>Server encounters a system error.</summary>
            ServerSystemError = -19999,
            /// <summary>Server failed to deserialize client data.</summary>
            RequestParamError = -10100,

            #region Login/Session Status Codes
            /// <summary>Requested player ID is not available at this time.</summary>
            NotAvailablePlayer = -10101,
            /// <summary>Requested player ID does not exist.</summary>
            MissingPlayer = -10102,
            /// <summary>Session ID expired.</summary>
            /// <remarks>There seems to be a bug in the client where it will not attempt to create a new login session when this error code is returned until the session was supposed to expire. This results in a partial hang.</remarks>
            ExpirationSession = -10103,
            /// <summary>Incorrect password.</summary>
            PassWordError = -10104,
            /// <summary>`seq` number does not match what is expected for the current session.</summary>
            SequenceIDError = -10107, // Not yet fully implemented.
            #endregion

            #region Atom Serial Status Codes
            /// <summary>Atom serial is invalid.</summary>
            InvalidSerialCode = -10105,
            /// <summary>Atom serial has already been used.</summary>
            UsedSerialCode = -10106,
            #endregion

            #region Web API Status Codes
            /// <summary>Error occurred during communication with the HSP web API.</summary>
            HspWebApiError = -10110,
            /// <summary>Error occurred during communication with the Apollo web API.</summary>
            ApolloWebApiError = -10115,
            #endregion

            #region Data Mismatch Status Codes
            DataMismatch = -30120,
            MasterDataMismatch = -10121,
            #endregion

            #region Insufficient Currency Status Codes
            /// <summary>Player does not have enough red star rings for the specified operation.</summary>
            /// <remarks>This error code will be displayed to the user if the client-side check fails for whatever reason.</remarks>
            NotEnoughRedStarRings = -20130,
            /// <summary>Player does not have enough rings for the specified operation.</summary>
            /// <remarks>This error code will be displayed to the user if the client-side check fails for whatever reason.</remarks>
            NotEnoughRings = -20131,
            /// <summary>Player does not have enough energy for the specified operation.</summary>
            /// <remarks>This error code will be displayed to the user if the client-side check fails for whatever reason.</remarks>
            NotEnoughEnergy = -20132,
            /// <summary>Player does not have enough boss challenge tokens for the specified operation.</summary>
            /// <remarks>This error code will be displayed to the user if the client-side check fails for whatever reason.</remarks>
            NotEnoughChallenge = -20133,
            #endregion

            #region Roulette Status Codes
            /// <summary>No more free spins and the player has no roulette tickets.</summary>
            /// <remarks>This error code will be displayed to the user if the client-side check fails for whatever reason.</remarks>
            RouletteUseLimit = -30401,
            /// <summary>Roulette board was reset.</summary>
            RouletteBoardReset = -30411,
            /// <summary>Character is maxed out.</summary>
            CharacterLevelLimit = -20601,
            /// <summary>Every single character and Chao is maxed out on the current account.</summary>
            AllChaoLevelLimit = -20602,
            #endregion

            #region Friend Status Codes
            /// <summary>Friend has already been invited.</summary>
            AlreadyInvitedFriend = -30801,
            /// <summary>Energy request has already been sent.</summary>
            AlreadyRequestedEnergy = -30901,
            /// <summary>Specified friend has already received energy.</summary>
            AlreadySentEnergy = -30902,
            /// <summary>Gift receiving failed.</summary>
            ReceiveFailureMessage = -30910,
            #endregion

            #region IAP Status Codes
            AlreadyExistedPrePurchase = -11001,
            AlreadyRemovedPrePurchase = -11002,
            InvalidReceiptData = -11003,
            AlreadyProcessedReceipt = -11004,
            EnergyLimitPurchaseTrigger = -21010,
            AmountExceedingLimit = -31001,
            #endregion

            #region Event Status Codes
            /// <summary>Specified event has not started yet (or is invalid).</summary>
            NotStartEvent = -10201,
            /// <summary>Specified event has already ended.</summary>
            AlreadyEndEvent = -10202,
            #endregion

            #region Username Status Codes
            UsernameInvalidChars = -40000, // for 2.1.0 and later
            UsernameTooLong = -40001, // for 2.1.0 and later
            UsernameHasNGWord = -40002, // for 2.1.0 and later
            #endregion

            #region Internal Use Status Codes
            /// <summary>Current client version needs to use the Application server instead of the Release or Staging servers.</summary>
            VersionForApplication = -999002,
            #endregion

            TimeOut = -7,
            OtherError = -8,
            NotReachability = -10,
            InvalidResponse = -20,
            ClientError = -400,
            InternalServerError = -500,
            HspPurchaseError = -600,
            ServerBusy = -700
        }

        public string errorMessage { get; set; }
        public Int64 closeTime { get; set; }
        public string seq { get; set; }
        public Int64 server_time { get; set; }
        public SRStatusCode statusCode { get; set; }

        /// <summary>
        /// The constructors for BaseInfo. If no parameters are specified, the default will be used.
        /// </summary>
        /// <param name="em">The error message.</param>
        /// <param name="ct">The close time.</param>
        /// <param name="s">The "seq". (honestly I have no idea what this is)</param>
        /// <param name="sc">The status code.</param>
        public BaseInfo(string em, long ct, string s, SRStatusCode sc)
        {
            errorMessage = em;
            closeTime = ct;
            seq = s;
            server_time = DateTimeOffset.Now.ToUnixTimeSeconds();
            statusCode = sc;
        }

        public BaseInfo()
        {
            errorMessage = "OK";
            closeTime = DateTimeOffset.Now.AddTicks(-1).AddDays(1).ToUnixTimeSeconds();
            seq = "0";
            server_time = DateTimeOffset.Now.ToUnixTimeSeconds();
            statusCode = SRStatusCode.ServerMaintenance;
        }
    }
}
