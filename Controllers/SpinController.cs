using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;
using static spikewall.Object.Item;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("Spin")]
    public class SpinController : ControllerBase
    {
        [HttpPost]
        [Route("getWheelOptions")]
        [Produces("text/json")]
        public JsonResult GetWheelOptions([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
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

            var populateStatus = playerState.Populate(conn, clientReq.userId);
            if (populateStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateStatus));
            }

            WheelOptions wheelOptions = new();
            wheelOptions.Populate(conn, clientReq.userId);

            WheelOptionsResponse wheelOptionsResponse = new()
            {
                wheelOptions = wheelOptions
            };

            return new JsonResult(EncryptedResponse.Generate(iv, wheelOptionsResponse));
        }

        [HttpPost]
        [Route("commitWheelSpin")]
        [Produces("text/json")]
        public JsonResult CommitWheelSpin([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<CommitWheelSpinRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            PlayerState playerState = new();

            var populatePlayerStatus = playerState.Populate(conn, clientReq.userId);
            if (populatePlayerStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populatePlayerStatus));
            }

            WheelOptions wheelOptions = new();
            wheelOptions.Populate(conn, clientReq.userId);

            var wonItemIndex = wheelOptions.itemWon;
            var wonItemID = wheelOptions.items[wonItemIndex];
            var wonItemCount = (ulong)wheelOptions.item[wonItemIndex];

            switch (wonItemID)
            {
                // Only add valid items to the item list (120000 - 120007)
                case > (long)ItemID.SubCharacter and < (long)ItemID.RingBonus:
                {
                    var itemSQL = Db.GetCommand(@"INSERT INTO `sw_itemownership` (
                                                                user_id, item_id
                                                            ) VALUES (
                                                                '{0}', '{1}'
                                                            );", clientReq.userId, wonItemID);
                    var insertCmd = new MySqlCommand(itemSQL, conn);

                    for (ulong i = 0; i < wonItemCount; i++)
                    {
                        insertCmd.ExecuteNonQuery();
                    }
                    wheelOptions.rouletteRank = 0;
                    break;
                }
                case (long)ItemID.RedStarRing:
                    playerState.numRedRings += wonItemCount;
                    wheelOptions.rouletteRank = 0;
                    break;
                case (long)ItemID.Ring:
                    playerState.numRings += wonItemCount;
                    wheelOptions.rouletteRank = 0;
                    break;
                case (long)ItemID.ItemRouletteRankUp when wheelOptions.rouletteRank != 2:
                    wheelOptions.rouletteRank++;
                    wheelOptions.numRemainingRoulette++;
                    break;
                case (long)ItemID.ItemRouletteRankUp:
                    // Jackpot won
                    playerState.numRings += (ulong)wheelOptions.numJackpotRing;
                    wheelOptions.rouletteRank = 0;
                    break;
                case 400000:
                    // FIXME: Stub
                    wheelOptions.numRemainingRoulette++;
                    wheelOptions.rouletteRank = 0;
                    break;
            }

            if (wheelOptions.numRemainingRoulette == playerState.numRouletteTicket)
            {
                wheelOptions.numRouletteToken--;
                playerState.numRouletteTicket--;
            }

            wheelOptions.numRemainingRoulette--;

            // Regenerate item list so the client's item list
            // doesn't become desynced from the roulette rank
            var getWheelOptionsStatus = WheelOptions.GetItemWheelOptions(conn, wheelOptions.rouletteRank, out long[] items, out long[] itemNum, out short[] itemWeight);
            if (getWheelOptionsStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            wheelOptions.items = items;
            wheelOptions.item = itemNum;
            wheelOptions.itemWeight = itemWeight;

            var savePlayerStatus = playerState.Save(conn, clientReq.userId);
            if (savePlayerStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, savePlayerStatus));
            }

            var saveWheelStatus = wheelOptions.Save(conn, clientReq.userId);
            if (saveWheelStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, saveWheelStatus));
            }

            CommitWheelSpinResponse commitWheelSpinResponse = new()
            {
                playerState = playerState,

                // FIXME: Missing ChaoState!!

                wheelOptions = wheelOptions
            };

            return new JsonResult(EncryptedResponse.Generate(iv, commitWheelSpinResponse));
        }
    }
}
