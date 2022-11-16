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

            Invincible = 120000,
            Shield,
            Magnet,
            Trampoline,
            Combo,
            Laser,
            Drill,
            Asteroid,
            RingBonus,
            DistanceBonus,
            AnimalBonus,

            RedStarRing = 900000,
            Ring = 910000
        }
    }
}
