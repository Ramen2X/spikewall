using spikewall.Object;

namespace spikewall.Response
{
    public class GetItemStockNumResponse : BaseResponse
    {
        public Item[] itemStockList { get; set; }

        public GetItemStockNumResponse()
        {
            itemStockList = Array.Empty<Item>();
        }
    }
}
