using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;
using System.Security.Cryptography;
using static spikewall.Object.Chao;
using static spikewall.Object.ChaoBase;
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

            var populateChaoState = PopulateChaoState(conn, clientReq.userId, out Chao[] chaoState);
            if (populateChaoState != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateChaoState));
            }

            WheelOptions wheelOptions = new();
            wheelOptions.Populate(conn, clientReq.userId, ref chaoState);

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

            var populateChaoState = PopulateChaoState(conn, clientReq.userId, out Chao[] chaoState);
            if (populateChaoState != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateChaoState));
            }

            WheelOptions wheelOptions = new();
            wheelOptions.Populate(conn, clientReq.userId, ref chaoState);

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
                case (long)ItemID.ItemRouletteRankUp when wheelOptions.rouletteRank != 2: // BIG or SUPER
                    wheelOptions.rouletteRank++;
                    wheelOptions.numRemainingRoulette++;
                    break;
                case (long)ItemID.ItemRouletteRankUp: // JACKPOT
                    playerState.numRings += (ulong)wheelOptions.numJackpotRing;
                    wheelOptions.rouletteRank = 0;
                    break;
                case (long)ItemID.NormalEgg: // normal buddy
                    var getNormalChaoPrizeSql = Db.GetCommand(@"SELECT * FROM `sw_chaoitemrouletteprizelist` ORDER BY (RAND() * chao_weight) DESC LIMIT 1");
                    var getNormalChaoPrizeCmd = new MySqlCommand(getNormalChaoPrizeSql, conn);
                    var getNormalChaoPrizeRdr = getNormalChaoPrizeCmd.ExecuteReader();
                    if (getNormalChaoPrizeRdr.HasRows)
                    {
                        Chao chao = new();
                        chao.chaoID = Convert.ToString(getNormalChaoPrizeRdr["chao_id"]);
                        var getChaoIndex = FindChaoInChaoState(Convert.ToInt32(chao.chaoID), chaoState);
                        LevelUpChao(conn, Convert.ToInt32(chao.chaoID), ref chaoState, out getChaoIndex);
                        if (chaoState[getChaoIndex].status == (sbyte)Chao.Status.MaxLevel)
                        {
                            playerState.chaoEggs += 1;
                        }
                        AddChaoToChaoState(conn, Convert.ToInt32(chao.chaoID), ref chaoState, clientReq.userId, ref getChaoIndex);
                        SaveChaoState(conn, clientReq.userId, chaoState);
                    }
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

                chaoState = chaoState,

                wheelOptions = wheelOptions
            };

            return new JsonResult(EncryptedResponse.Generate(iv, commitWheelSpinResponse));
        }
    }
}
