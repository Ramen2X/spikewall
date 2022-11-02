namespace spikewall.Object
{
    public class Ticker
    {
        // The ID of this ticker.
        public byte? id { get; set; }

        // The epoch time that this
        // ticker starts displaying in game.
        public long? start { get; set; }

        // The epoch time that this
        // ticker stops displaying in game.
        public long? end { get; set; }

        // The message to be
        // displayed by this ticker.
        public string? param { get; set; }
    }
}
