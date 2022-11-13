namespace spikewall.Object
{
    /// <summary>
    /// Class for Incentives, a type of item that
    /// is given out to players as rewards for completing
    /// certain objectives, like completing the Daily Challenge,
    /// completing an episode in the Story Mode, etc.
    /// </summary>
    public class Incentive : Item
    {
        public long? numIncentiveCont { get; set; }
    }
}
