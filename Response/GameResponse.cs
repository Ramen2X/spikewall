using spikewall.Object;

namespace spikewall.Response
{
    public class FreeItemListResponse : BaseResponse
    {
        public Item[]? freeItemList { get; set; }

        public FreeItemListResponse()
        {
            // This will tell the client that no free items
            // are available. While this is a good default,
            // there should be a separate constructor that
            // takes the free item list from the database.
            freeItemList = new Item[0];
        }
    }

    public class QuickActStartResponse : BaseResponse
    {
        public Campaign[]? campaignList { get; set; }
        public PlayerState? playerState { get; set; }
    }

    public class ActStartResponse : QuickActStartResponse
    {
        public MileageFriend[]? distanceFriendList { get; set; }
    }

    public class QuickPostGameResultsResponse : BaseResponse
    {
        public PlayerState? playerState { get; set; }
        public Chao[]? chao { get; set; }
        public Incentive[]? dailyChallengeIncentive { get; set; }
        public Character[]? characterState { get; set; }
        public string[]? messageList { get; set; }
        public string[]? operatorMessageList { get; set; }
        public long? totalMessage { get; set; }
        public long? totalOperatorMessage { get; set; }
        public Character[]? playCharacterState { get; set; }
    }
}
