namespace spikewall.Request
{
    public class GetMessageRequest : BaseRequest
    {
        public int[]? MessageIDs { get; set; }
        public int[]? OperatorMessageIDs { get; set; }
    }
}
