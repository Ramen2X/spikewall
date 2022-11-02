using spikewall.Object;
using System;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing daily challenge data
    /// </summary>
    public class MileageDataResponse : BaseResponse
    {
        public MileageFriend[]? mileageFriendList { get; set; }
        public MileageMapState? mileageMapState { get; set; }

        public MileageDataResponse()
        {
            // FIXME: Set up remaining defaults
            mileageFriendList = new MileageFriend[0];
            mileageMapState = new MileageMapState();
        }
    }
}
