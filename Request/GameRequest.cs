using spikewall.Object;
using System.Text.Json.Serialization;

namespace spikewall.Request
{
    /// <summary>
    /// Class which represents data from a QuickActStart request.
    /// QuickActStart is sent when beginning a Timed Mode run.
    /// </summary>
    public class QuickActStartRequest : BaseRequest
    {
        // Actually means "modifier", misspelling is actually
        // in the game though so we have to keep it like this.
        public Int64[]? modifire { get; set; }
        // This is not always sent, so it needs to be
        // specifically handled during deserialization.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Int64? tutorial { get; set; }
    }

    /// <summary>
    /// Class which represents data from an ActStart request.
    /// ActStart is sent when beginning a Story Mode/Endless run.
    /// </summary>
    public class ActStartRequest : QuickActStartRequest
    {
        public MileageFriend[]? distanceFriendList { get; set; }
    }

    /// <summary>
    /// Class which represents data from a QuickPostGameResults request.
    /// QuickPostGameResults is sent when finishing a Timed Mode run.
    /// </summary>
    public class QuickPostGameResultsRequest : BaseRequest
    {
        public Int64? score { get; set; }
        public Int64? numRings { get; set; }
        public Int64? numFailureRings { get; set; }
        public Int64? numRedStarRings { get; set; }
        public Int64? distance { get; set; }
        public Int64? dailyChallengeValue { get; set; }
        public Int64? dailyChallengeComplete { get; set; }
        public Int64? numAnimals { get; set; }
        public Int64? maxCombo { get; set; }
        public Int64? closed { get; set; }
        public string? cheatResult { get; set; }
    }

    /// <summary>
    /// Class which represents data from a PostGameResults request.
    /// PostGameResults is sent when finishing a Story Mode/Endless run.
    /// </summary>
    public class PostGameResultsRequest : QuickPostGameResultsRequest
    {
        public Int64? bossDestroyed { get; set; }
        public Int64? chapterClear { get; set; }
        public Int64? getChaoEgg { get; set; }
        public Int64? numBossAttack { get; set; }
        public Int64? reachPoint { get; set; }

    }

    /// <summary>
    /// Class which represents data from a MileageRewardRequest.
    /// </summary>
    public class MileageRewardRequest : BaseRequest
    {
        public Int64? episode { get; set; }
        public Int64? chapter { get; set; }
    }
}
