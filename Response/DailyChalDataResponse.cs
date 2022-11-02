using spikewall.Object;
using System;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing daily challenge data
    /// </summary>
    public class DailyChalDataResponse : BaseResponse
    {
        public Incentive[]? incentiveList { get; set; }
        public long? incentiveListCont { get; set; }
        public long? numDilayChalCont { get; set; }
        public long? numDailyChalDay { get; set; }
        public long? maxDailyChalDay { get; set; }
        public long? chalEndTime { get; set; }

        public DailyChalDataResponse()
        {
            // FIXME: Set up remaining defaults
            incentiveList = new Incentive[0];
        }
    }
}
