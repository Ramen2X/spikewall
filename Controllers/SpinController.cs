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

            var populateChaoState = PopulateChaoState(conn, clientReq.userId, out Chao[] chaoState);
            if (populateChaoState != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateChaoState));
            }

            WheelOptions wheelOptions = new();
            wheelOptions.Populate(conn, clientReq.userId);

            var wonItemIndex = wheelOptions.itemWon;
            var wonItemID = wheelOptions.items[wonItemIndex];
            var wonItemCount = (ulong)wheelOptions.item[wonItemIndex];

            var chaostatesql = Db.GetCommand("SELECT * FROM `sw_chaostates WHERE user_id = '{0}'", clientReq.userId);
            var chaostatecommand = new MySqlCommand(chaostatesql, conn);
            var chaostatereader = chaostatecommand.ExecuteReader();

            var itemchaosql = Db.GetCommand("SELECT * FROM `sw_chao` WHERE on_item_roulette = '{1}'", 1);
            var chaoCmd = new MySqlCommand(itemchaosql, conn);
            var chaoRdr = chaoCmd.ExecuteReader();
            List<Chao> chaoItemRoulettePrizeList;
            if (chaoRdr.HasRows)
            {
                // Convert ChaoState to list so we can append to it
                chaoItemRoulettePrizeList = new(chaoState);

                // Read row
                chaoRdr.Read();

                Chao c = new()
                {
                    chaoID = Convert.ToString(chaoRdr["id"]),
                };
                chaoRdr.Close();

                // Insert our chao into the Prize List
                chaostatesql = Db.GetCommand(@"INSERT INTO `sw_chaoitemrouletteprizelist` (
                                              chao_id
                                          ) VALUES (
                                              '{0}'
                                          );", c.chaoID);
                var insertCmd = new MySqlCommand(chaostatesql, conn);
                insertCmd.ExecuteNonQuery();

                chaoItemRoulettePrizeList.Add(c);

                // Convert prizeList back to array to return it
                chaoState = chaoItemRoulettePrizeList.ToArray();
            }


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
                    int chaoPrizeWinIndex = RandomNumberGenerator.GetInt32(0, chaoState.Length);
                    var wonChaoPrize = chaoState[chaoPrizeWinIndex];
                    for (int i = 0; i < chaoState.Length; i++)
                    {
                        if (chaoPrizeWinIndex == i)
                        {
                            if (wonChaoPrize.status == (sbyte)Chao.Status.NotOwned)
                            {
                                wonChaoPrize.status = (sbyte)Chao.Status.Owned;
                            }
                            else if (wonChaoPrize.status == (sbyte)Chao.Status.Owned && wonChaoPrize.level < 10)
                            {
                                wonChaoPrize.level += 1;
                            }
                            else if (wonChaoPrize.level == 10)
                            {
                                wonChaoPrize.status = (sbyte)Chao.Status.MaxLevel;
                            }
                            else
                            {
                                playerState.chaoEggs += 1;
                            }
                        }
                    }
                    AddChaoToChaoState(conn, Convert.ToInt32(wonChaoPrize.chaoID), ref chaoState, clientReq.userId, ref chaoPrizeWinIndex);
                    SaveChaoState(conn, clientReq.userId, chaoState);
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
