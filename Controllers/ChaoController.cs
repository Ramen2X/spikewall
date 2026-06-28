using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;
using System.Security.Cryptography;
using static spikewall.Object.Chao;
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

            var chaostatesql = Db.GetCommand("SELECT * FROM `sw_chaostates WHERE user_id = '{0}'", clientReq.userId);
            var chaostatecommand = new MySqlCommand(chaostatesql, conn);
            var chaostatereader = chaostatecommand.ExecuteReader();

            var chaosql = Db.GetCommand("SELECT * FROM `sw_chao` WHERE on_chao_roulette = '{1}'", 1);
            var chaoCmd = new MySqlCommand(chaosql, conn);
            var chaoRdr = chaoCmd.ExecuteReader();
            List<Chao> chaoPrizeList;
            if (chaoRdr.HasRows)
            {
                // Convert ChaoState to list so we can append to it
                chaoPrizeList = new(chaoState);

                // Read row
                chaoRdr.Read();

                Chao c = new()
                {
                    chaoID = Convert.ToString(chaoRdr["id"])
                };


                chaoRdr.Close();

                // Insert our chao into the Prize List
                chaostatesql = Db.GetCommand(@"INSERT INTO `sw_rouletteprizelist` (
                                              chao_id
                                          ) VALUES (
                                              '{0}'
                                          );", c.chaoID);
                var insertCmd = new MySqlCommand(chaostatesql, conn);
                insertCmd.ExecuteNonQuery();

                chaoPrizeList.Add(c);

                // Convert prizeList back to array to return it
                chaoState = chaoPrizeList.ToArray();
            }

            var characterstatesql = Db.GetCommand("SELECT * FROM `sw_characterstates WHERE user_id = '{0}'", clientReq.userId);
            var characterstatecommand = new MySqlCommand(characterstatesql, conn);
            var characterstatereader = characterstatecommand.ExecuteReader();

            var charactersql = Db.GetCommand("SELECT * FROM `sw_character` WHERE on_chao_roulette = '{0}'", 1);
            var characterCmd = new MySqlCommand(chaosql, conn);
            var characterRdr = characterCmd.ExecuteReader();
            List<Character> characterPrizeList;
            if (characterRdr.HasRows)
            {
                // Convert CharacterState to list so we can append to it
                characterPrizeList = new(characterState);

                // Read row
                characterRdr.Read();

                Character c = new()
                {
                    characterId = Convert.ToInt32(characterRdr["id"])
                };
                characterRdr.Close();

                // Insert our character into the Prize List
                characterstatesql = Db.GetCommand(@"INSERT INTO `sw_rouletteprizelist` (
                                              chao_id
                                          ) VALUES (
                                              '{0}'
                                          );", c.characterId);
                var insertCmd = new MySqlCommand(characterstatesql, conn);
                insertCmd.ExecuteNonQuery();

                characterPrizeList.Add(c);

                // Convert prizeList back to array to return it
                characterState = characterPrizeList.ToArray();
            }

            var prizeList = new Chao[chaoState.Length + characterState.Length];
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

            ChaoSpinResult chaoSpinResult = new();
            WheelOptions wheelOptions = new();

            var wonChaoIndex = chaoSpinResult.ItemWon;
            var wonItemID = (ulong)wheelOptions.items[wonChaoIndex];
            var wonChaoPrize = chaoSpinResult.PrizeWon[wonItemID];

            var chaostatesql = Db.GetCommand("SELECT * FROM `sw_chaostates WHERE user_id = '{0}'", clientReq.userId);
            var chaostatecommand = new MySqlCommand(chaostatesql, conn);
            var chaostatereader = chaostatecommand.ExecuteReader();

            var chaosql = Db.GetCommand("SELECT * FROM `sw_chao` WHERE on_chao_roulette = '{1}'", 1);
            var chaoCmd = new MySqlCommand(chaosql, conn);
            var chaoRdr = chaoCmd.ExecuteReader();
            List<Chao> chaoPrizeList;
            if (chaoRdr.HasRows)
            {
                // Convert ChaoState to list so we can append to it
                chaoPrizeList = new(chaoState);

                // Read row
                chaoRdr.Read();

                Chao c = new()
                {
                    chaoID = Convert.ToString(chaoRdr["id"])
                };


                chaoRdr.Close();

                // Insert our chao into the Prize List
                chaostatesql = Db.GetCommand(@"INSERT INTO `sw_rouletteprizelist` (
                                              chao_id
                                          ) VALUES (
                                              '{0}'
                                          );", c.chaoID);
                var insertCmd = new MySqlCommand(chaostatesql, conn);
                insertCmd.ExecuteNonQuery();

                chaoPrizeList.Add(c);

                // Convert prizeList back to array to return it
                chaoState = chaoPrizeList.ToArray();
            }

            var characterstatesql = Db.GetCommand("SELECT * FROM `sw_characterstates WHERE user_id = '{0}'", clientReq.userId);
            var characterstatecommand = new MySqlCommand(characterstatesql, conn);
            var characterstatereader = characterstatecommand.ExecuteReader();

            var charactersql = Db.GetCommand("SELECT * FROM `sw_character` WHERE on_chao_roulette = '{0}'", 1);
            var characterCmd = new MySqlCommand(chaosql, conn);
            var characterRdr = characterCmd.ExecuteReader();
            List<Character> characterPrizeList;
            if (characterRdr.HasRows)
            {
                // Convert CharacterState to list so we can append to it
                characterPrizeList = new(characterState);

                // Read row
                characterRdr.Read();

                Character c = new()
                {
                    characterId = Convert.ToInt32(characterRdr["id"])
                };
                characterRdr.Close();

                // Insert our character into the Prize List
                characterstatesql = Db.GetCommand(@"INSERT INTO `sw_rouletteprizelist` (
                                              chao_id
                                          ) VALUES (
                                              '{0}'
                                          );", c.characterId);
                var insertCmd = new MySqlCommand(characterstatesql, conn);
                insertCmd.ExecuteNonQuery();

                characterPrizeList.Add(c);

                // Convert prizeList back to array to return it
                characterState = characterPrizeList.ToArray();
            }

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
                playerState.chaoEggs -= 10;
            }

            int[] chaoPrizeWinIndex = new int[5];
            int[] characterPrizeWinIndex = new int[5];
            for (int i = 0; i < requestCount; i++)
            {
                switch (wonItemID)
                {
                    case (ulong)Item.ItemID.RareEgg:
                    case (ulong)Item.ItemID.SuperRareEgg:
                        chaoPrizeWinIndex[i] = RandomNumberGenerator.GetInt32(0, chaoState.Length);
                        for (int j = 0; j < chaoPrizeWinIndex.Length; j++)
                        {
                            if (chaoState[chaoPrizeWinIndex[j]].status == (sbyte)Chao.Status.NotOwned)
                            {
                                chaoState[chaoPrizeWinIndex[j]].status = (sbyte)Chao.Status.Owned;
                            }

                            else if (chaoState[chaoPrizeWinIndex[j]].status == (sbyte)Chao.Status.Owned && chaoState[chaoPrizeWinIndex[j]].level < 10)
                            {
                                chaoState[chaoPrizeWinIndex[j]].level += 1;
                            }
                             else if (chaoState[chaoPrizeWinIndex[j]].level == 10)
                            {
                                chaoState[chaoPrizeWinIndex[j]].status = (sbyte)Chao.Status.MaxLevel;
                            }
                            else
                            {
                                playerState.chaoEggs += 1;
                            }
                        }
                        SaveChaoState(conn, clientReq.userId, chaoState);
                        break;
                    case (ulong)Item.ItemID.CharacterEgg:
                        characterPrizeWinIndex[i] = RandomNumberGenerator.GetInt32(0, chaoState.Length);
                        for (int j = 0; j < characterPrizeWinIndex.Length; j++)
                        {
                            if (characterState[characterPrizeWinIndex[j]].status == (sbyte)Character.Status.Locked)
                            {
                                chaoState[characterPrizeWinIndex[j]].status = (sbyte)Character.Status.Unlocked;
                            }
                            else if (characterState[characterPrizeWinIndex[j]].status == (sbyte)Character.Status.Unlocked && characterState[characterPrizeWinIndex[j]].star < 10)
                            {
                                characterState[characterPrizeWinIndex[j]].star += 1;
                            }
                            else if (characterState[characterPrizeWinIndex[j]].star == 10)
                            {
                                characterState[characterPrizeWinIndex[j]].status = (sbyte)Character.Status.MaxLevel;
                            }
                            else
                            {
                                playerState.numRedRings += 50;
                                playerState.numRings += 10_000;
                                playerState.chaoEggs += 1;
                            }
                        }
                        SaveCharacterState(conn, clientReq.userId, characterState);
                        break;
                }
            }
            return new JsonResult(EncryptedResponse.Generate(iv, new ChaoWheelSpinResponse()));
        }

    }
}
