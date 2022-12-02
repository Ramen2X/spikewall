using Microsoft.AspNetCore.Mvc;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("RaidbossSpin")]
    public class RaidBossSpinController : ControllerBase
    {
        /// <summary>
        /// Endpoint that gives the client the number of
        /// Item Roulette Tickets, Premium Roulette Tickets,
        /// and Chao Eggs that the player has.
        /// </summary>
        [HttpPost]
        [Route("getItemStockNum")]
        [Produces("text/json")]
        public JsonResult GetItemStockNum([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();

            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            PlayerState playerState = new();
            playerState.Populate(conn, clientReq.userId);

            GetItemStockNumResponse getItemStockNumResponse = new();

            Item[] itemStockList = new Item[3];

            itemStockList[0] = new()
            {
                itemId = (long)Item.ItemID.SpecialEgg,
                numItem = playerState.chaoEggs
            };

            itemStockList[1] = new()
            {
                itemId = (long)Item.ItemID.ItemRouletteTicket,
                numItem = playerState.numRouletteTicket
            };

            itemStockList[2] = new()
            {
                itemId = (long)Item.ItemID.PremiumRouletteTicket,
                numItem = playerState.numChaoRouletteTicket
            };

            getItemStockNumResponse.itemStockList = itemStockList;

            return new JsonResult(EncryptedResponse.Generate(iv, getItemStockNumResponse));
        }
    }
}
