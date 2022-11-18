namespace spikewall.Object
{
    /// <summary>
    /// Class for Information, an object that contains
    /// a message or an image, used to provide
    /// announcements and other information to the client.
    /// </summary>
    public class Information
    {
        // The ID of this information.
        public long id { get; set; }

        // The priority of this information.
        // (determines what the client shows first)
        public sbyte priority { get; set; }

        // Unix epoch time that this information
        // should start being shown to the player.
        public long start { get; set; }

        // Unix epoch time that this information
        // should stop being shown to the player.
        public long end { get; set; }

        // The message that this information contains.
        public string param { get; set; }

        public enum InformationType
        {
            Message,
            Image,
            Feed,
            ShopCancel,
            FeedURL,
            FeedRoulette,
            FeedShop,
            FeedEvent,
            FeedEventList,
            URL,
            Roulette,
            Shop,
            Event,
            RouletteInfo,
            QuickInfo,
            CountryMessage,
            CountryImage
        }

        public enum DisplayType
        {
            EveryDay,
            Once,
            Always,
            OnlyInfoPage
        }
    }

    /// <summary>
    /// Unsure what this is for right now, but it is
    /// commonly grouped together with normal information.
    /// </summary>
    public class OperatorInformation
    {
        // The ID of this operator information.
        public long id { get; set; }

        // The message that this operator information contains.
        public string content { get; set; }
    }
}
