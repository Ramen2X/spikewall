using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;
using System.Security.Cryptography;
using static spikewall.Object.Chao;
using static spikewall.Object.ChaoBase;
using static spikewall.Object.Character;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("Chao")]
    public class ChaoController : ControllerBase
    {
        [HttpPost]
        [Route("getChaoWheelOptions")]
        [Produces("text/json")]
        public JsonResult GetChaoWheelOptions([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<RedstarExchangeListRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
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
            var populateCharacterState = PopulateCharacterState(conn, clientReq.userId, out Character[] characterState);
            if (populateCharacterState != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateChaoState));
            }

            ChaoWheelOptions chaoWheelOptions = new();
            chaoWheelOptions.PopulateChaoWheel(conn, clientReq.userId);

            ChaoWheelOptionsResponse chaoWheelOptionsResponse = new()
            {
                chaoWheelOptions = chaoWheelOptions
            };

            return new JsonResult(EncryptedResponse.Generate(iv, chaoWheelOptionsResponse));
        }

        [HttpPost]
        [Route("getPrizeChaoWheelSpin")]
        [Produces("text/json")]
        public JsonResult GetPrizeChaoWheelOptions([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<RedstarExchangeListRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            PlayerState playerState = new();

            var populateStatus = playerState.Populate(conn, clientReq.userId);
            if (populateStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateStatus));
            }

            ChaoWheelOptions.GetPrizeChaoWheelOptions(conn, out Chao[] rareChaoPrizeList, out Chao[] superRareChaoPrizeList, out Character[] characterPrizeList);

            var prizeList = new Chao[rareChaoPrizeList.Length + superRareChaoPrizeList.Length + characterPrizeList.Length];

            PrizeChaoWheelSpinResponse chaoWheelSpinResponse = new(prizeList)
            {
                prizeList = prizeList
            };

            return new JsonResult(EncryptedResponse.Generate(iv, chaoWheelSpinResponse));
        }

        [HttpPost]
        [Route("commitChaoWheelSpin")]
        [Produces("text/json")]
        public JsonResult CommitChaoWheelSpin([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<RedstarExchangeListRequest>(conn, param, secure, key);
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
            var populateCharacterState = PopulateCharacterState(conn, clientReq.userId, out Character[] characterState);
            if (populateCharacterState != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateChaoState));
            }

            ChaoWheelOptions chaoWheelOptions = new();
            chaoWheelOptions.PopulateChaoWheel(conn, clientReq.userId);
            chaoWheelOptions.chaoRouletteType = (long)ChaoWheelOptions.ChaoRouletteType.Normal;

            ChaoSpinResult chaoSpinResult = new();
            WheelOptions wheelOptions = new();

            ChaoWheelOptions.GetChaoWheelOptions(conn, chaoWheelOptions.chaoRouletteType, out long[] chaoRarity, out short[] chaoWeight);
            ChaoWheelOptions.AdjustChaoWeights(conn, ref chaoRarity, ref chaoWeight, ref chaoState, ref characterState);

            CommitChaoWheelSpinRequest commitChaoWheelSpinRequest = new();
            var requestCount = commitChaoWheelSpinRequest.count;

            if (playerState.chaoEggs < 10)
            {
                if (playerState.numChaoRouletteTicket > 0)
                {
                    playerState.numChaoRouletteTicket -= 1 * requestCount;
                }
                else
                {
                    playerState.numRedRings -= 50 * (ulong)requestCount;
                }
            }
            else
            {
                chaoWheelOptions.chaoRouletteType = (long)ChaoWheelOptions.ChaoRouletteType.Special;
                requestCount = 1;
                playerState.chaoEggs -= 10;
            }
            
            for (int i = 0; i < requestCount; i++)
            {
                var wonChaoIndex = chaoSpinResult.ItemWon;
                var wonItemID = (ulong)wheelOptions.items[wonChaoIndex];

                var sql = Db.GetCommand(@"SELECT * FROM `sw_chaorouletteprizelist`");
                var command = new MySqlCommand(sql, conn);

                var prizeRdr = command.ExecuteReader();

                while (prizeRdr.Read())
                {
                    switch (wonItemID)
                    {
                        case (ulong)Item.ItemID.RareEgg:
                            var getRareChaoPrizeSql = Db.GetCommand(@"SELECT * FROM `sw_chaorouletteprizelist` WHERE rarity = '{0}' ORDER BY RAND() LIMIT 1", (ulong)Item.ItemID.RareEgg);
                            var getRareChaoPrizeCommand = new MySqlCommand(getRareChaoPrizeSql, conn);
                            var rareChaoPrizeRdr = command.ExecuteReader();

                            if (rareChaoPrizeRdr.HasRows)
                            {
                                Chao c = new();

                                c.chaoID = Convert.ToString(rareChaoPrizeRdr["chao_id"]);
                                var getChaoIndex = FindChaoInChaoState(Convert.ToInt32(c.chaoID), chaoState);
                                if (chaoState[getChaoIndex].status == (sbyte)Chao.Status.MaxLevel)
                                {
                                    playerState.chaoEggs += 1;
                                }
                                else LevelUpChao(conn, Convert.ToInt32(c.chaoID), ref chaoState, out getChaoIndex);

                                AddChaoToChaoState(conn, Convert.ToInt32(c.chaoID), ref chaoState, clientReq.userId, ref getChaoIndex);
                                SaveChaoState(conn, clientReq.userId, chaoState);
                                rareChaoPrizeRdr.Close();
                            }
                            
                            break;
                        case (ulong)Item.ItemID.SuperRareEgg:
                            var getSRareChaoPrizeSql = Db.GetCommand(@"SELECT * FROM `sw_chaorouletteprizelist` WHERE rarity = '{0}' ORDER BY RAND() LIMIT 1", (ulong)Item.ItemID.SuperRareEgg);
                            var getSRareChaoPrizeCommand = new MySqlCommand(getSRareChaoPrizeSql, conn);
                            var sRareChaoPrizeRdr = getSRareChaoPrizeCommand.ExecuteReader();
                            if (sRareChaoPrizeRdr.HasRows)
                            {
                                Chao c = new();

                                c.chaoID = Convert.ToString(sRareChaoPrizeRdr["chao_id"]);
                                var getChaoIndex = FindChaoInChaoState(Convert.ToInt32(c.chaoID), chaoState);
                                if (chaoState[getChaoIndex].status == (sbyte)Chao.Status.MaxLevel)
                                {
                                    playerState.chaoEggs += 1;
                                }
                                else LevelUpChao(conn, Convert.ToInt32(c.chaoID), ref chaoState, out getChaoIndex);

                                AddChaoToChaoState(conn, Convert.ToInt32(c.chaoID), ref chaoState, clientReq.userId, ref getChaoIndex);
                                SaveChaoState(conn, clientReq.userId, chaoState);
                                sRareChaoPrizeRdr.Close();
                            }
                            break;
                        case (ulong)Item.ItemID.CharacterEgg:
                            var characterPrizeSql = Db.GetCommand(@"SELECT * FROM `sw_chaorouletteprizelist` WHERE rarity = '{0}' ORDER BY RAND() LIMIT 1", (ulong)Item.ItemID.CharacterEgg);
                            var characterPrizeCommand = new MySqlCommand(characterPrizeSql, conn);
                            var characterPrizeRdr = characterPrizeCommand.ExecuteReader();
                            if (characterPrizeRdr.HasRows)
                            {
                                Character c = new();

                                c.characterId = Convert.ToInt32(characterPrizeRdr["chao_id"]);
                                var getCharacterIndex = FindCharacterInCharacterState(c.characterId, characterState);
                                if (characterState[getCharacterIndex].status == (sbyte)Character.Status.Locked)
                                {
                                    characterState[getCharacterIndex].status = (sbyte)Character.Status.Unlocked;
                                }
                                else if (characterState[getCharacterIndex].star < 10)
                                {
                                    characterState[getCharacterIndex].star += 1;
                                }
                                else if (characterState[getCharacterIndex].star == 10)
                                {
                                    characterState[getCharacterIndex].status = (sbyte)Character.Status.MaxLevel;
                                }
                                else
                                {
                                    playerState.numRedRings += 50;
                                    playerState.numRings += 10_000;
                                    playerState.chaoEggs += 1;
                                }
                                characterPrizeRdr.Close();
                                AddCharacterToCharacterState(conn, c.characterId, ref characterState, clientReq.userId, ref getCharacterIndex);
                                SaveCharacterState(conn, clientReq.userId, characterState);
                            }
                            break;
                    }
                }
                prizeRdr.Close();
            }

            // Regenerate chao item list so the client's chao item list
            // doesn't become desynced from the current premium roulette rank
            var getChaoWheelOptionsStatus = ChaoWheelOptions.GetChaoWheelOptions(conn, chaoWheelOptions.chaoRouletteType, out chaoRarity, out chaoWeight);
            if (getChaoWheelOptionsStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            chaoWheelOptions.rarity = chaoRarity;
            chaoWheelOptions.itemWeight = chaoWeight;

            var savePlayerStatus = playerState.Save(conn, clientReq.userId);
            if (savePlayerStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, savePlayerStatus));
            }

            var saveChaoWheelStatus = chaoWheelOptions.Save(conn, clientReq.userId);
            if (saveChaoWheelStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, saveChaoWheelStatus));
            }

            ChaoWheelSpinResponse chaoWheelSpinResponse = new()
            {
                playerState = playerState,

                characterState = characterState,

                chaoState = chaoState,

                ChaoWheelOptions = chaoWheelOptions
            };
            return new JsonResult(EncryptedResponse.Generate(iv, chaoWheelSpinResponse));
        }
    }
}
