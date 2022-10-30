namespace spikewall.Object
{
    public class ChaoBase
    {
        // The ID of this chao.
        public string? chaoID { get; set; }

        // The rarity of this chao.
        // Rarities range from N to SR.
        public Int64? rarity { get; set; }

        // Apparently unused?
        public Int64? hidden { get; set; }
    }

    public class Chao : ChaoBase
    {
        // Whether or not the chao is unlocked.
        public int? status { get; set; }

        // The level of this chao.
        public Int64? level { get; set; }

        // Unsure of this right now.
        public Int64? setStatus { get; set; }

        // Unsure about this. Whether or not
        // the chao is unlocked is already noted
        // with "status", so it cannot be that?
        public Int64? acquired { get; set; }
    }
}
