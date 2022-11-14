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
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class QuickPostGameResultsRequest : BaseRequest
    {
        public ulong? score { get; set; }
        public ulong? numRings { get; set; }
        public ulong? numFailureRings { get; set; }
        public ulong? numRedStarRings { get; set; }
        public ulong? distance { get; set; }
        public string? dailyChallengeValue { get; set; }
        public sbyte? dailyChallengeComplete { get; set; }
        public ulong? numAnimals { get; set; }
        public ulong? maxCombo { get; set; }
        public sbyte? closed { get; set; }
        public string? cheatResult { get; set; }
    }

    /// <summary>
    /// Class which represents data from a PostGameResults request.
    /// PostGameResults is sent when finishing a Story Mode/Endless run.
    /// </summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class PostGameResultsRequest : QuickPostGameResultsRequest
    {
        public sbyte? bossDestroyed { get; set; }
        public sbyte? chapterClear { get; set; }
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
