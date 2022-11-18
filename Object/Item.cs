namespace spikewall.Object
{
    public class Item
    {
        public long? itemId { get; set; }
        public long? numItem { get; set; }

        /// <summary>
        /// Enum that contains the names 
        /// and IDs of all relevant items.
        /// </summary>
        public enum ItemID
        {
            None = -1,

            ScoreBoost = 110000,
            SupportSpring,
            SubCharacter,

            Invincible = 120000,
            Shield,
            Magnet,
            Spring,
            ComboBonus,
            Laser,
            Drill,
            Asteroid,
            RingBonus,
            DistanceBonus,
            AnimalBonus,

            RedStarRing = 900000,
            Ring = 910000
        }

        public Item(long itemId, long numItem)
        {
            this.itemId = itemId;
            this.numItem = numItem;
        }

        public Item()
        {
            this.itemId = 0;
            this.numItem = 0;
        }
    }
}
