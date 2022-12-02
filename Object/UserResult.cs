namespace spikewall.Object
{
    /// <summary>
    /// Class for miscellaneous stats which aren't returned alongside the PlayerState.
    /// </summary>
    public class UserResult
    {
        /// <summary>The highest Total Score the player has ever achieved.</summary>
        public long? totalSumHightScore { get; set; }
        /// <summary>The highest Timed Mode Total Score the player has ever achieved.</summary>
        public long? quickTotalSumHightScore { get; set; }

        /// <summary>The total number of rings ever accumulated by the player.</summary>
        public long? numTakeAllRings { get; set; }
        /// <summary>The total number of red rings ever accumulated by the player.</summary>
        public long? numTakeAllRedRings { get; set; }

        /// <summary>The total number of Chao Roulette spins the player has ever done.</summary>
        public int? numChaoRoulette { get; set; }
        /// <summary>The total number of Item Roulette spins the player has ever done.</summary>
        public int? numItemRoulette { get; set; }

        /// <summary>The total number of jackpots won on the Item Roulette.</summary>
        public int? numJackPot { get; set; }
        /// <summary>The biggest jackpot the player has received, in rings.</summary>
        public long? numMaximumJackPotRings { get; set; }

        /// <summary>The total number of times the player has given support to other players.</summary>
        public int? numSupport { get; set; }

        public UserResult()
        {
            totalSumHightScore = 0;
            quickTotalSumHightScore = 0;
            numTakeAllRings = 0;
            numTakeAllRedRings = 0;
            numChaoRoulette = 0;
            numItemRoulette = 0;
            numJackPot = 0;
            numMaximumJackPotRings = 0;
            numSupport = 0;
        }
    }
}
