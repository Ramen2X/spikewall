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
            items = new string[0];
            item = new long[0];
            itemWeight = new long[0];
            itemList = new Item[0];
        }
    }
}
