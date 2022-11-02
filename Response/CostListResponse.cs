using spikewall.Object;
using System;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing cost list (FIXME: Elaborate)
    /// </summary>
    public class CostListResponse : BaseResponse
    {
        // FIXME: This is an array but shouldn't actually be strings, set up "Cost" object
        public string[]? consumedCostList { get; set; }

        public CostListResponse()
        {
            this.consumedCostList = new string[0];
        }
    }
}
