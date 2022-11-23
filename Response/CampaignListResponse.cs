using spikewall.Object;

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
            this.campaignList = Array.Empty<Campaign>();
        }
    }
}
