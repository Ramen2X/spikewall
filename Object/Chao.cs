namespace spikewall.Object
{
    public class ChaoBase
    {
        // The ID of this chao.
        public string? chaoID { get; set; }

        // The rarity of this chao.
        // Rarities range from N to SR.
        public long? rarity { get; set; }

        // Apparently unused?
        public long? hidden { get; set; }
    }

    public class Chao : ChaoBase
    {
        // Whether or not the chao is unlocked.
        public sbyte? status { get; set; }

        // The level of this chao.
        public sbyte? level { get; set; }

        // Unsure of this right now.
        public long? setStatus { get; set; }

        // Unsure about this. Whether or not
        // the chao is unlocked is already noted
        // with "status", so it cannot be that?
        public long? acquired { get; set; }
    }

    public class ChaoWheelOptions
    {
        public long[]? rarity { get; set; }
        public long[]? itemWeight { get; set; }
        public Campaign[]? campaignList { get; set; }
        public long? spinCost { get; set; }
        public long? chaoRouletteType { get; set; }
        public long? numSpecialEgg { get; set; }
        public long? rouletteAvailable { get; set; }
        public long? numChaoRouletteToken { get; set; }
        public long? numChaoRoulette { get; set; }
        public long? startTime { get; set; }
        public long? endTime { get; set; }

        public ChaoWheelOptions()
        {
            // FIXME: Dummy data
            rarity = new long[0];
            itemWeight = new long[0];
            campaignList = new Campaign[0];
            spinCost = 0;
            chaoRouletteType = 0;
            numSpecialEgg = 0;
            rouletteAvailable = 0;
            numChaoRouletteToken = 0;
            numChaoRoulette = 0;
            startTime = 0;
            endTime = 0;
        }
    }
}
