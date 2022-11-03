namespace spikewall.Object
{
    public class LoginBonusStatus
    {
        // Number of logins.
        public long? numLogin { get; set; }
        
        // Number of bonuses?
        public long? numBonus { get; set; }

        // Last time a bonus had been obtained.
        public long? lastBonusTime { get; set; }
    }

    public class LoginBonusReward
    {
        public SelectReward[]? selectRewardList { get; set; }
    }
}
