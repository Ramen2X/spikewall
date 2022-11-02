using spikewall.Object;
using System;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing red star exchange list (FIXME: Elaborate)
    /// </summary>
    public class RedstarExchangeListResponse : BaseResponse
    {
        // FIXME: Messages shouldn't actually be strings, set up "StoreItem" object
        public string[]? itemList { get; set; }
        public long? totalItems { get; set; }
        public long? monthPurchase { get; set; }
        public string? birthday { get; set; }

        public RedstarExchangeListResponse()
        {
            itemList = new string[0];
            totalItems = 0;
            monthPurchase = 0;
            birthday = "1900-1-1";
        }
    }
}
