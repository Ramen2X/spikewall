namespace spikewall.Response
{
    /// <summary>
    /// Response containing event list (FIXME: Elaborate)
    /// </summary>
    public class EventListResponse : BaseResponse
    {
        // FIXME: This is an array but shouldn't actually be strings, set up "Event" object
        public string[]? eventList { get; set; }

        public EventListResponse()
        {
            this.eventList = Array.Empty<string>();
        }
    }
}
