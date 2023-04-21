using spikewall.Debug;
using spikewall.Object;
using System.Text.Json.Serialization;

namespace spikewall.Request
{
    /// <summary>
    /// Class which represents data from a Quick/ActStart request.
    /// QuickActStart is sent when beginning a Timed Mode run.
    /// ActStart is sent when beginning a Story Mode/Endless run.
    /// </summary>
    public class ActStartRequest : BaseRequest
    {
        // Actually means "modifier", misspelling is actually
        // in the game though so we have to keep it like this.
        public long[]? modifire { get; set; }
        // This is not always sent, so it needs to be
        // specifically handled during deserialization.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? tutorial { get; set; }

        // This is only sent as part of ActStartRequest (not quick/timed)
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MileageFriend[]? distanceFriendList { get; set; }
    }

    /// <summary>
    /// Class which represents data from a QuickPostGameResults request.
    /// QuickPostGameResults is sent when finishing a Timed Mode run.
    /// </summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class QuickPostGameResultsRequest : BaseRequest
    {
        public ulong score { get; set; }
        public ulong numRings { get; set; }
        public ulong numFailureRings { get; set; }
        public ulong numRedStarRings { get; set; }
        public ulong distance { get; set; }
        public string dailyChallengeValue { get; set; }
        public sbyte dailyChallengeComplete { get; set; }
        public ulong numAnimals { get; set; }
        public ulong maxCombo { get; set; }
        public sbyte closed { get; set; }
        public string cheatResult { get; set; }

        /// <summary>
        /// Handle the original client's anti-cheat mechanism "cheatResult", which is
        /// a set of bit flags that indicate which cheats the player used in a run.
        /// </summary>
        public void CheckCheatResult(string uid)
        {
            if (!cheatResult.Equals("00000000"))
            {
                if (cheatResult[(int)CheatResult.DistanceMismatch].Equals('1'))
                {
                    DebugHelper.Log("Distance mismatch detected for user " + uid, 1);
                    distance = 0;
                }

                if (cheatResult[(int)CheatResult.RingMismatch].Equals('1'))
                {
                    DebugHelper.Log("Ring mismatch detected for user " + uid, 1);
                    numRings = 0;
                    numFailureRings = 0;
                }

                if (cheatResult[(int)CheatResult.StageScoreMismatch].Equals('1'))
                {
                    DebugHelper.Log("Stage score mismatch detected for user " + uid, 1);
                    score = 0;
                }

                if (cheatResult[(int)CheatResult.AnimalMismatch].Equals('1'))
                {
                    DebugHelper.Log("Animal mismatch detected for user " + uid, 1);
                    numAnimals = 0;
                }

                if (cheatResult[(int)CheatResult.EventObjectMismatch].Equals('1'))
                {
                    DebugHelper.Log("Event object mismatch detected for user " + uid, 1);
                    // FIXME: Figure out how to penalize event objects
                }

                if (cheatResult[(int)CheatResult.TotalScoreMismatch].Equals('1'))
                {
                    DebugHelper.Log("Total score mismatch detected for user " + uid, 1);
                    score = 0;
                }

                if (cheatResult[(int)CheatResult.RedStarRingMismatch].Equals('1'))
                {
                    DebugHelper.Log("Red Star Ring mismatch detected for user " + uid, 1);
                    numRedStarRings = 0;
                }
            }
        }
        
        private enum CheatResult
        {
            DistanceMismatch,
            RingMismatch,
            StageScoreMismatch,
            AnimalMismatch,
            EventObjectMismatch,
            TotalScoreMismatch,
            RedStarRingMismatch
        }
    }

    /// <summary>
    /// Class which represents data from a PostGameResults request.
    /// PostGameResults is sent when finishing a Story Mode/Endless run.
    /// </summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class PostGameResultsRequest : QuickPostGameResultsRequest
    {
        public sbyte bossDestroyed { get; set; }
        public sbyte chapterClear { get; set; }
        public long getChaoEgg { get; set; }
        public long numBossAttack { get; set; }
        public long reachPoint { get; set; }
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
