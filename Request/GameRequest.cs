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
        public long[]? modifire { get; set; }
        // This is not always sent, so it needs to be
        // specifically handled during deserialization.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? tutorial { get; set; }
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
        public string? score { get; set; }
        public string? numRings { get; set; }
        public string? numFailureRings { get; set; }
        public string? numRedStarRings { get; set; }
        public string? distance { get; set; }
        public string? dailyChallengeValue { get; set; }
        public sbyte? dailyChallengeComplete { get; set; }
        public string? numAnimals { get; set; }
        public string? maxCombo { get; set; }
        public sbyte? closed { get; set; }
        public string? cheatResult { get; set; }
    }

    /// <summary>
    /// Class which represents data from a PostGameResults request.
    /// PostGameResults is sent when finishing a Story Mode/Endless run.
    /// </summary>
    public class PostGameResultsRequest : QuickPostGameResultsRequest
    {
        public long? bossDestroyed { get; set; }
        public long? chapterClear { get; set; }
        public long? getChaoEgg { get; set; }
        public long? numBossAttack { get; set; }
        public long? reachPoint { get; set; }

    }

    /// <summary>
    /// Class which represents data from a MileageRewardRequest.
    /// </summary>
    public class MileageRewardRequest : BaseRequest
    {
        public sbyte? episode { get; set; }
        public sbyte? chapter { get; set; }
    }
}
