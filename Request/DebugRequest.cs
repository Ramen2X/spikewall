using spikewall.Object;

namespace spikewall.Request
{
    /// <summary>
    /// Requests for the various debug endpoints
    /// that are used by the client's debugging functions.
    /// </summary>
    public class UpdateMileageDataRequest : BaseRequest
    {
        public MileageMapState? mileageMapState { get; set; }
    }
    public class UpdateUserDataRequest : BaseRequest
    {
        public int? addRank { get; set; }
    }
}
