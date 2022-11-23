namespace spikewall.Object
{
    public class WheelOptions
    {
        public string[]? items { get; set; }
        public long[]? item { get; set; }
        public long[]? itemWeight { get; set; }
        public long? itemWon { get; set; }
        public long? nextFreeSpin { get; set; }
        public long? spinCost { get; set; }
        public long? rouletteRank { get; set; }
        public long? numRouletteToken { get; set; }
        public long? numJackpotRing { get; set; }
        public long? numRemainingRoulette { get; set; }
        public Item[]? itemList { get; set; }

        public WheelOptions()
        {
            // FIXME: Set up remaining defaults
            items = Array.Empty<string>();
            item = Array.Empty<long>();
            itemWeight = Array.Empty<long>();
            itemList = Array.Empty<Item>();
        }
    }
}
