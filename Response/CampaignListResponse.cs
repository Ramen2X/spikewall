using spikewall.Object;
using System;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing list of campaigns
    /// </summary>
    public class CampaignListResponse : BaseResponse
    {
        public Campaign[]? campaignList { get; set; }

        public CampaignListResponse()
        {
            this.campaignList = new Campaign[0];
        }
    }
}
