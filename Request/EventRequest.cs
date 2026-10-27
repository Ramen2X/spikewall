namespace spikewall.Request
{
    /// <summary>
    /// This class will grab the corresponding Event based on the Event's ID that is currently running
    /// </summary>
    public class EventRequest : BaseRequest
    {
        public int EventID { get; set; }
    }
}
