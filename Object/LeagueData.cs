using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using spikewall.Response;
using System.Security.Cryptography;
using static spikewall.Object.Character;

namespace spikewall.Object
{
    public class LeagueData
    {
        public long? leagueId { get; set; }
        public long? groupId { get; set; }
        public long? numUp { get; set; }
        public long? numDown { get; set; }
        public long? numGroupMember { get; set; }
        public long? numLeagueMember { get; set; }
        public OperatorScore[]? highScoreOpe { get; set; }
        public OperatorScore[]? totalScoreOpe { get; set; }

        /// <summary>
        /// Enum that contains the names 
        /// and IDs of all Runners League Ranks.
        /// The number at the end signifies the number of stars in each League Rank.
        /// </summary>
        public enum LeagueID
        {
            F1,
            F2,
            F3,
            E1,
            E2,
            E3,
            D1,
            D2,
            D3,
            C1,
            C2,
            C3,
            B1,
            B2,
            B3,
            A1,
            A2,
            A3,
            S1,
            S2,
            S3
        }

        /// <summary>
        /// Enum that contains the the mode types
        /// and IDs of the Runners League Ranks.
        /// </summary>
        public enum RankingMode
        {
            Endless,
            Quick
        }

        public static SRStatusCode GenerateEndlessLeagueData(MySqlConnection conn, string uid, out LeagueData currentEndlessLeague)
        {
            PlayerState playerState = new();

            var generateCurrentEndlessLeagueDataSql = Db.GetCommand(@"SELECT * FROM `sw_endlessleaguedata` WHERE league_id = '{0}'", playerState.rankingLeague);
            var generateCurrentEndlessLeagueDataCmd = new MySqlCommand(generateCurrentEndlessLeagueDataSql, conn);
            var generateCurrentEndlessLeagueDataReader = generateCurrentEndlessLeagueDataCmd.ExecuteReader();

            currentEndlessLeague = new();

            if (generateCurrentEndlessLeagueDataReader.HasRows)
            {
                currentEndlessLeague.leagueId = Convert.ToInt64(generateCurrentEndlessLeagueDataReader["league_id"]);
                currentEndlessLeague.groupId = Convert.ToInt64(generateCurrentEndlessLeagueDataReader["group_id"]);
                currentEndlessLeague.numUp = Convert.ToInt64(generateCurrentEndlessLeagueDataReader["num_up"]);
                currentEndlessLeague.numDown = Convert.ToInt64(generateCurrentEndlessLeagueDataReader["num_down"]);
                currentEndlessLeague.numGroupMember = Convert.ToInt64(generateCurrentEndlessLeagueDataReader["num_in_group"]);
                currentEndlessLeague.numLeagueMember = Convert.ToInt64(generateCurrentEndlessLeagueDataReader["num_in_league"]);
                currentEndlessLeague.highScoreOpe = OperatorScore.GenerateEndlessLeagueHighScorePrizes(conn, currentEndlessLeague.leagueId);
                currentEndlessLeague.highScoreOpe = OperatorScore.GenerateEndlessLeagueTotalScorePrizes(conn, currentEndlessLeague.leagueId);
            }

            conn.Close();

            SaveEndlessLeagueData(conn, uid, ref currentEndlessLeague);

            return SRStatusCode.Ok;
        }

        public static SRStatusCode GenerateQuickLeagueData(MySqlConnection conn, string uid, out LeagueData currentQuickLeague)
        {
            PlayerState playerState = new();

            var generateCurrentQuickLeagueDataSql = Db.GetCommand(@"SELECT * FROM `sw_endlessleaguedata` WHERE league_id = '{0}'", playerState.quickRankingLeague);
            var generateCurrentQuickLeagueDataCmd = new MySqlCommand(generateCurrentQuickLeagueDataSql, conn);
            var generateCurrentQuickLeagueDataReader = generateCurrentQuickLeagueDataCmd.ExecuteReader();

            currentQuickLeague = new();

            if (generateCurrentQuickLeagueDataReader.HasRows)
            {
                currentQuickLeague.leagueId = Convert.ToInt64(generateCurrentQuickLeagueDataReader["league_id"]);
                currentQuickLeague.groupId = Convert.ToInt64(generateCurrentQuickLeagueDataReader["group_id"]);
                currentQuickLeague.numUp = Convert.ToInt64(generateCurrentQuickLeagueDataReader["num_up"]);
                currentQuickLeague.numDown = Convert.ToInt64(generateCurrentQuickLeagueDataReader["num_down"]);
                currentQuickLeague.numGroupMember = Convert.ToInt64(generateCurrentQuickLeagueDataReader["num_in_group"]);
                currentQuickLeague.numLeagueMember = Convert.ToInt64(generateCurrentQuickLeagueDataReader["num_in_league"]);
                currentQuickLeague.highScoreOpe = OperatorScore.GenerateQuickLeagueHighScorePrizes(conn, currentQuickLeague.leagueId);
                currentQuickLeague.highScoreOpe = OperatorScore.GenerateQuickLeagueTotalScorePrizes(conn, currentQuickLeague.leagueId);
            }

            conn.Close();

            SaveQuickLeagueData(conn, uid, ref currentQuickLeague);

            return SRStatusCode.Ok;
        }

        public static SRStatusCode SaveEndlessLeagueData(MySqlConnection conn, string uid, ref LeagueData currentEndlessLeague)
        {
            GetStartAndEndTimesForEndlessLeague(conn, (long)currentEndlessLeague.leagueId, (long)currentEndlessLeague.groupId, out long endlessStartTime, out long endlessResetTime);

            var endlessLeagueDataSql = Db.GetCommand(@"UPDATE `sw_endlessleaguedata` SET league_id = '{0}', start_time = '{1}', end_time = '{2}', group_id = '{3}', num_up = '{4}', num_down = '{5}', num_in_group = '{6}', num_in_league = '{7}' WHERE id = '{8}'", currentEndlessLeague.leagueId, currentEndlessLeague.groupId, endlessStartTime, endlessResetTime, currentEndlessLeague.numUp, currentEndlessLeague.numDown, currentEndlessLeague.numGroupMember, currentEndlessLeague.numLeagueMember, uid);
            var endlessLeagueDataCommand = new MySqlCommand(endlessLeagueDataSql, conn);

            int rowsAffected = endlessLeagueDataCommand.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                return SRStatusCode.DataMismatch;
            }
            return SRStatusCode.Ok;
        }
        public static SRStatusCode SaveQuickLeagueData(MySqlConnection conn, string uid, ref LeagueData currentQuickLeague)
        {

            GetStartAndEndTimesForQuickLeague(conn, (long)currentQuickLeague.leagueId, (long)currentQuickLeague.groupId, out long quickStartTime, out long quickResetTime);
            var quickLeagueDataSql = Db.GetCommand(@"UPDATE `sw_endlessleaguedata` SET league_id = '{0}', start_time = '{1}', end_time = '{2}', group_id = '{3}', num_up = '{4}', num_down = '{5}', num_in_group = '{6}', num_in_league = '{7}' WHERE id = '{8}''", currentQuickLeague.leagueId, currentQuickLeague.groupId, quickStartTime, quickResetTime, currentQuickLeague.numUp, currentQuickLeague.numDown, currentQuickLeague.numGroupMember, currentQuickLeague.numLeagueMember, uid);
            var quickLeagueDataCommand = new MySqlCommand(quickLeagueDataSql, conn);

            int rowsAffected = quickLeagueDataCommand.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                return SRStatusCode.DataMismatch;
            }
            return SRStatusCode.Ok;
        }

        public static SRStatusCode GenerateEndlessLeagueDataList(MySqlConnection conn, string uid, out LeagueData[] endlessLeague)
        {
            List<LeagueData> endlessLeagueDataList = new List<LeagueData>();
            PlayerState playerState = new();

            var generateEndlessLeagueDataSql = Db.GetCommand(@"SELECT * FROM `sw_endlessleaguedata`");
            var generateEndlessLeagueDataCmd = new MySqlCommand(generateEndlessLeagueDataSql, conn);
            var generateEndlessLeagueDataReader = generateEndlessLeagueDataCmd.ExecuteReader();

            LeagueData endlessLeagueData = new();

            while (generateEndlessLeagueDataReader.Read())
            {
                endlessLeagueData.leagueId = Convert.ToInt64(generateEndlessLeagueDataReader["league_id"]);
                endlessLeagueData.groupId = Convert.ToInt64(generateEndlessLeagueDataReader["group_id"]);
                endlessLeagueData.numUp = Convert.ToInt64(generateEndlessLeagueDataReader["num_up"]);
                endlessLeagueData.numDown = Convert.ToInt64(generateEndlessLeagueDataReader["num_down"]);
                endlessLeagueData.numGroupMember = Convert.ToInt64(generateEndlessLeagueDataReader["num_in_group"]);
                endlessLeagueData.numLeagueMember = Convert.ToInt64(generateEndlessLeagueDataReader["num_in_league"]);
                endlessLeagueData.highScoreOpe = OperatorScore.GenerateEndlessLeagueHighScorePrizes(conn, endlessLeagueData.leagueId);
                endlessLeagueData.highScoreOpe = OperatorScore.GenerateEndlessLeagueTotalScorePrizes(conn, endlessLeagueData.leagueId);

                endlessLeagueDataList.Add(endlessLeagueData);
            }

            generateEndlessLeagueDataReader.Close();

            endlessLeague = endlessLeagueDataList.ToArray();
            return SRStatusCode.Ok;
        }


        public static SRStatusCode GenerateQuickLeagueDataList(MySqlConnection conn, string uid, out LeagueData[] quickLeague)
        {
            List<LeagueData> quickLeagueDataList = new List<LeagueData>();
            PlayerState playerState = new();

            var generateQuickLeagueDataSql = Db.GetCommand(@"SELECT * FROM `sw_quickleaguedata`");
            var generateQuickLeagueDataCmd = new MySqlCommand(generateQuickLeagueDataSql, conn);
            var generateQuickLeagueDataReader = generateQuickLeagueDataCmd.ExecuteReader();

            while (generateQuickLeagueDataReader.Read())
            {
                LeagueData quickLeagueData = new();
                quickLeagueData.leagueId = Convert.ToInt64(generateQuickLeagueDataReader["league_id"]);
                quickLeagueData.groupId = Convert.ToInt64(generateQuickLeagueDataReader["group_id"]);
                quickLeagueData.numUp = Convert.ToInt64(generateQuickLeagueDataReader["num_up"]);
                quickLeagueData.numDown = Convert.ToInt64(generateQuickLeagueDataReader["num_down"]);
                quickLeagueData.numGroupMember = Convert.ToInt64(generateQuickLeagueDataReader["num_in_group"]);
                quickLeagueData.numLeagueMember = Convert.ToInt64(generateQuickLeagueDataReader["num_in_league"]);
                quickLeagueData.highScoreOpe = OperatorScore.GenerateQuickLeagueHighScorePrizes(conn, quickLeagueData.leagueId);
                quickLeagueData.highScoreOpe = OperatorScore.GenerateQuickLeagueTotalScorePrizes(conn, quickLeagueData.leagueId);

                quickLeagueDataList.Add(quickLeagueData);
            }
            generateQuickLeagueDataReader.Close();

            quickLeague = quickLeagueDataList.ToArray();
            return SRStatusCode.Ok;
        }

        public static SRStatusCode GetStartAndEndTimesForEndlessLeague(MySqlConnection conn, long leagueId, long groupId, out long endlessStartTime, out long endlessResetTime)
        {
            endlessStartTime = 0;
            endlessResetTime = 0;

            var endlessLeagueTimesSql = Db.GetCommand(@"SELECT start_time, end_time FROM `sw_endlessleaguedata WHERE league_id = '{0}' AND group_id = '{1}'", leagueId, groupId);
            var endlessLeagueTimesCommand = new MySqlCommand(endlessLeagueTimesSql, conn);
            var endlessLeagueTimesReader = endlessLeagueTimesCommand.ExecuteReader();

            if (endlessLeagueTimesReader.HasRows)
            {
                endlessStartTime = Convert.ToInt64("start_time");
                endlessResetTime = Convert.ToInt64("end_time");
            }
            conn.Close();
            return SRStatusCode.Ok;
        }
        public static SRStatusCode GetStartAndEndTimesForQuickLeague(MySqlConnection conn, long leagueId, long groupId, out long quickStartTime, out long quickResetTime)
        {
            quickStartTime = 0;
            quickResetTime = 0;

            var quickLeagueTimesSql = Db.GetCommand(@"SELECT start_time, end_time FROM `sw_quickleaguedata WHERE league_id = '{0}' AND group_id = '{1}'", leagueId, groupId);
            var quickLeagueTimesCommand = new MySqlCommand(quickLeagueTimesSql, conn);
            var quickLeagueTimesReader = quickLeagueTimesCommand.ExecuteReader();

            if (quickLeagueTimesReader.HasRows)
            {
                quickStartTime = Convert.ToInt64("start_time");
                quickResetTime = Convert.ToInt64("end_time");
            }
            conn.Close();
            return SRStatusCode.Ok;
        }

        public static SRStatusCode GetEndlessHighScores(MySqlConnection conn, string uid, out LeaderboardEntry playerEntry, out LeaderboardEntry[] endlessLeaderboard, out long endlessLeaderboardPlayers)
        {
            PlayerState playerState = new();
            var populateStatus = playerState.Populate(conn, uid);

            var endlessHighScoresSql = Db.GetCommand("SELECT * FROM `sw_players` ORDER BY story_high_score DESC, highest_story_total_score DESC");
            var endlessHighScoresCommand = new MySqlCommand(endlessHighScoresSql, conn);
            var endlessHighScoresReader = endlessHighScoresCommand.ExecuteReader();

            List<LeaderboardEntry> leaderboardEntryList = new List<LeaderboardEntry>();

            while (endlessHighScoresReader.Read())
            {
                LeaderboardEntry leaderboardEntry = new()
                {
                    friendId = Convert.ToString("id"),
                    name = Convert.ToString("username"),
                    grade = Convert.ToInt64("ranking_league"),
                    rankingScore = Convert.ToUInt64("ranking_league_group"),
                    numRank = Convert.ToInt64("num_rank"),
                    loginTime = Convert.ToInt64("last_login"),
                    charaId = Convert.ToString("main_chara_id"),
                    subCharaId = Convert.ToString("sub_chara_id"),
                    mainChaoId = Convert.ToString("main_chao_id"),
                    subChaoId = Convert.ToString("sub_chao_id"),
                    language = Convert.ToInt32("language"),
                    league = Convert.ToInt64("ranking_league"),
                    maxScore = Convert.ToUInt64("story_high_score")
                };

                leaderboardEntryList.Add(leaderboardEntry);
            }

            endlessLeaderboard = leaderboardEntryList.ToArray();
            endlessLeaderboardPlayers = endlessLeaderboard.Count();

            endlessHighScoresReader.Close();

            var endlessOwnHighScoreSql = Db.GetCommand("SELECT * FROM `sw_players` WHERE id='{0}'", uid);
            var endlessOwnHighScoreCommand = new MySqlCommand(endlessOwnHighScoreSql, conn);
            var endlessOwnHighScoreReader = endlessOwnHighScoreCommand.ExecuteReader();

            playerEntry = new();

            if (endlessOwnHighScoreReader.HasRows)
            {
                LeaderboardEntry leaderboardEntry = new()
                {
                    friendId = Convert.ToString("id"),
                    name = Convert.ToString("username"),
                    grade = Convert.ToInt64("ranking_league"),
                    rankingScore = Convert.ToUInt64("ranking_league_group"),
                    numRank = Convert.ToInt64("num_rank"),
                    loginTime = Convert.ToInt64("last_login"),
                    charaId = Convert.ToString("main_chara_id"),
                    subCharaId = Convert.ToString("sub_chara_id"),
                    mainChaoId = Convert.ToString("main_chao_id"),
                    subChaoId = Convert.ToString("sub_chao_id"),
                    language = Convert.ToInt32("language"),
                    league = Convert.ToInt64("ranking_league"),
                    maxScore = Convert.ToUInt64("story_high_score")
                };

                playerEntry = leaderboardEntry;
            }

            conn.Close();
            return SRStatusCode.Ok;
        }

        public static SRStatusCode GetQuickHighScores(MySqlConnection conn, string uid, out LeaderboardEntry playerEntry, out LeaderboardEntry[] quickLeaderboard, out long quickLeaderboardPlayers)
        {
            PlayerState playerState = new();
            var populateStatus = playerState.Populate(conn, uid);

            var quickHighScoresSql = Db.GetCommand("SELECT * FROM `sw_players` ORDER BY quick_high_score DESC, highest_quick_total_score DESC");
            var quickHighScoresCommand = new MySqlCommand(quickHighScoresSql, conn);
            var quickHighScoresReader = quickHighScoresCommand.ExecuteReader();

            List<LeaderboardEntry> leaderboardEntryList = new List<LeaderboardEntry>();

            while (quickHighScoresReader.Read())
            {
                LeaderboardEntry leaderboardEntry = new()
                {
                    friendId = Convert.ToString("id"),
                    name = Convert.ToString("username"),
                    grade = Convert.ToInt64("quick_ranking_league"),
                    rankingScore = Convert.ToUInt64("quick_ranking_league_group"),
                    numRank = Convert.ToInt64("num_rank"),
                    loginTime = Convert.ToInt64("last_login"),
                    charaId = Convert.ToString("main_chara_id"),
                    subCharaId = Convert.ToString("sub_chara_id"),
                    mainChaoId = Convert.ToString("main_chao_id"),
                    subChaoId = Convert.ToString("sub_chao_id"),
                    language = Convert.ToInt32("language"),
                    league = Convert.ToInt64("quick_ranking_league"),
                    maxScore = Convert.ToUInt64("quick_high_score")
                };

                leaderboardEntryList.Add(leaderboardEntry);
            }

            quickLeaderboard = leaderboardEntryList.ToArray();

            quickLeaderboardPlayers = quickLeaderboard.Count();

            quickHighScoresReader.Close();

            var quickOwnHighScoreSql = Db.GetCommand("SELECT * FROM `sw_players` WHERE id='{0}'", uid);
            var quickOwnHighScoreCommand = new MySqlCommand(quickOwnHighScoreSql, conn);
            var quickOwnHighScoreReader = quickOwnHighScoreCommand.ExecuteReader();

            playerEntry = new();

            if (quickOwnHighScoreReader.HasRows)
            {
                LeaderboardEntry leaderboardEntry = new()
                {
                    friendId = Convert.ToString("id"),
                    name = Convert.ToString("username"),
                    grade = Convert.ToInt64("quick_ranking_league"),
                    rankingScore = Convert.ToUInt64("quick_ranking_league_group"),
                    numRank = Convert.ToInt64("num_rank"),
                    loginTime = Convert.ToInt64("last_login"),
                    charaId = Convert.ToString("main_chara_id"),
                    subCharaId = Convert.ToString("sub_chara_id"),
                    mainChaoId = Convert.ToString("main_chao_id"),
                    subChaoId = Convert.ToString("sub_chao_id"),
                    language = Convert.ToInt32("language"),
                    league = Convert.ToInt64("quick_ranking_league"),
                    maxScore = Convert.ToUInt64("quick_high_score")
                };

                playerEntry = leaderboardEntry;
            }

            conn.Close();
            return SRStatusCode.Ok;
        }

        public static SRStatusCode GetEndlessLeagueHighScores(MySqlConnection conn, string uid, long lbtype, long leagueId, long leagueGroup, out LeaderboardEntry playerEntry, out long endlessEntryCount, out LeaderboardEntry[] endlessLeaderboardEntries)
        {
            PlayerState playerState = new();
            var populateStatus = playerState.Populate(conn, uid);

            var endlessLeagueHighScoresSql = Db.GetCommand("SELECT * FROM `sw_players` ORDER BY league_high_score DESC WHERE ranking_league = '{0}', ranking_league_group_id = '{1}'", leagueId, leagueGroup);
            var endlessLeagueHighScoresCommand = new MySqlCommand(endlessLeagueHighScoresSql, conn);
            var endlessLeagueHighScoresReader = endlessLeagueHighScoresCommand.ExecuteReader();

            List<LeaderboardEntry> leaderboardEntryList = new List<LeaderboardEntry>();

            while (endlessLeagueHighScoresReader.Read())
            {
                LeaderboardEntry leaderboardEntry = new()
                {
                    friendId = Convert.ToString("id"),
                    name = Convert.ToString("username"),
                    grade = Convert.ToInt64("ranking_league"),
                    rankingScore = Convert.ToUInt64("ranking_league_group"),
                    numRank = Convert.ToInt64("num_rank"),
                    loginTime = Convert.ToInt64("last_login"),
                    charaId = Convert.ToString("main_chara_id"),
                    subCharaId = Convert.ToString("sub_chara_id"),
                    mainChaoId = Convert.ToString("main_chao_id"),
                    subChaoId = Convert.ToString("sub_chao_id"),
                    language = Convert.ToInt32("language"),
                    league = Convert.ToInt64("ranking_league"),
                    maxScore = Convert.ToUInt64("league_high_score")
                };

                leaderboardEntryList.Add(leaderboardEntry);
            }

            endlessLeaderboardEntries = leaderboardEntryList.ToArray();
            endlessEntryCount = endlessLeaderboardEntries.Count();

            endlessLeagueHighScoresReader.Close();

            var endlessOwnHighLeagueScoreSql = Db.GetCommand("SELECT * FROM `sw_players` WHERE id='{0}'", uid);
            var endlessOwnHighLeagueScoreCommand = new MySqlCommand(endlessOwnHighLeagueScoreSql, conn);
            var endlessOwnHighLeagueScoreReader = endlessOwnHighLeagueScoreCommand.ExecuteReader();

            playerEntry = new();

            if (endlessOwnHighLeagueScoreReader.HasRows)
            {
                LeaderboardEntry leaderboardEntry = new()
                {
                    friendId = Convert.ToString("id"),
                    name = Convert.ToString("username"),
                    grade = Convert.ToInt64("ranking_league"),
                    rankingScore = Convert.ToUInt64("ranking_league_group"),
                    numRank = Convert.ToInt64("num_rank"),
                    loginTime = Convert.ToInt64("last_login"),
                    charaId = Convert.ToString("main_chara_id"),
                    subCharaId = Convert.ToString("sub_chara_id"),
                    mainChaoId = Convert.ToString("main_chao_id"),
                    subChaoId = Convert.ToString("sub_chao_id"),
                    language = Convert.ToInt32("language"),
                    league = Convert.ToInt64("ranking_league"),
                    maxScore = Convert.ToUInt64("league_high_score")
                };

                playerEntry = leaderboardEntry;
            }
            conn.Close();

            return SRStatusCode.Ok;
        }
        public static SRStatusCode GetQuickLeagueHighScores(MySqlConnection conn, string uid, long lbtype, long leagueId, long leagueGroup, out LeaderboardEntry playerEntry, out long quickEntryCount, out LeaderboardEntry[] quickLeaderboardEntries)
        {
            PlayerState playerState = new();
            var populateStatus = playerState.Populate(conn, uid);

            var quickLeagueHighScoresSql = Db.GetCommand("SELECT * FROM `sw_players` ORDER BY quick_league_high_score DESC");
            var quickLeagueHighScoresCommand = new MySqlCommand(quickLeagueHighScoresSql, conn);
            var quickLeagueHighScoresReader = quickLeagueHighScoresCommand.ExecuteReader();

            List<LeaderboardEntry> leaderboardEntryList = new List<LeaderboardEntry>();

            while (quickLeagueHighScoresReader.Read())
            {
                LeaderboardEntry leaderboardEntry = new()
                {
                    friendId = Convert.ToString("id"),
                    name = Convert.ToString("username"),
                    grade = Convert.ToInt64("quick_ranking_league"),
                    rankingScore = Convert.ToUInt64("quick_ranking_league_group"),
                    numRank = Convert.ToInt64("num_rank"),
                    loginTime = Convert.ToInt64("last_login"),
                    charaId = Convert.ToString("main_chara_id"),
                    subCharaId = Convert.ToString("sub_chara_id"),
                    mainChaoId = Convert.ToString("main_chao_id"),
                    subChaoId = Convert.ToString("sub_chao_id"),
                    language = Convert.ToInt32("language"),
                    league = Convert.ToInt64("quick_ranking_league"),
                    maxScore = Convert.ToUInt64("quick_league_high_score")
                };

                leaderboardEntryList.Add(leaderboardEntry);
            }

            quickLeaderboardEntries = leaderboardEntryList.ToArray();
            quickEntryCount = quickLeaderboardEntries.Count();

            var quickOwnHighLeagueScoreSql = Db.GetCommand("SELECT * FROM `sw_players` WHERE id='{0}'", uid);
            var quickOwnHighLeagueScoreCommand = new MySqlCommand(quickOwnHighLeagueScoreSql, conn);
            var quickOwnHighLeagueScoreReader = quickOwnHighLeagueScoreCommand.ExecuteReader();

            playerEntry = new();

            if (quickOwnHighLeagueScoreReader.HasRows)
            {
                LeaderboardEntry leaderboardEntry = new()
                {
                    friendId = Convert.ToString("id"),
                    name = Convert.ToString("username"),
                    grade = Convert.ToInt64("quick_ranking_league"),
                    rankingScore = Convert.ToUInt64("quick_ranking_league_group"),
                    numRank = Convert.ToInt64("num_rank"),
                    loginTime = Convert.ToInt64("last_login"),
                    charaId = Convert.ToString("main_chara_id"),
                    subCharaId = Convert.ToString("sub_chara_id"),
                    mainChaoId = Convert.ToString("main_chao_id"),
                    subChaoId = Convert.ToString("sub_chao_id"),
                    language = Convert.ToInt32("language"),
                    league = Convert.ToInt64("quick_ranking_league"),
                    maxScore = Convert.ToUInt64("quick_league_high_score")
                };

                playerEntry = leaderboardEntry;
            }

            return SRStatusCode.Ok;
        }

        public static SRStatusCode GetNumberOfPlayers(MySqlConnection conn, out long numberofPlayers)
        {
            var numberofPlayersSql = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_players`");
            numberofPlayers = Convert.ToInt64(numberofPlayersSql);
            return SRStatusCode.Ok;
        }

        public static SRStatusCode GetNumberOfEndlessRunnersLeaguePlayers(MySqlConnection conn, out long numberOfEndlessRunnersLeaguePlayers)
        {
            LeagueData leagueData = new();

            var numberofEndlessRunnersLeaguePlayersSql = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_players` ORDER BY league_high_score DESC, story_total_score DESC WHERE quick_ranking_league = '{0}' AND ranking_league_group ='{1}'", leagueData.leagueId, leagueData.groupId);
            numberOfEndlessRunnersLeaguePlayers = Convert.ToInt64(numberofEndlessRunnersLeaguePlayersSql);
            return SRStatusCode.Ok;
        }
        public static SRStatusCode GetNumberOfQuickRunnersLeaguePlayers(MySqlConnection conn, out long numberOfQuickRunnersLeaguePlayers)
        {
            LeagueData leagueData = new();

            var numberofQuickRunnersLeaguePlayersSql = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_players` ORDER BY quick_league_high_score DESC, quick_total_score DESC WHERE quick_ranking_league = '{0}' AND quick_ranking_league_group ='{1}'", leagueData.leagueId, leagueData.groupId);
            numberOfQuickRunnersLeaguePlayers = Convert.ToInt64(numberofQuickRunnersLeaguePlayersSql);
            return SRStatusCode.Ok;
        }

        public static SRStatusCode CalculateEndlessRunnersLeague(MySqlConnection conn, string uid)
        {
            PlayerState playerState = new();
            var populateStatus = playerState.Populate(conn, uid);
            if (populateStatus != SRStatusCode.Ok)
            {
                return populateStatus;
            }

            DateTimeOffset leagueReset = new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                0, 0, 0, 0).AddDays(7);

            if (DateTime.Now >= leagueReset)
            {
                LeagueData endlessLeague = new();
                var playerSql = Db.GetCommand(@"SELECT *, rank() OVER (PARTITION BY ranking_league ORDER BY ranking_league_group ASC, league_high_score DESC) AS league_rank FROM `sw_players` WHERE id='{0}' AND ranking_league='{1}' AND ranking_league_group = '{2}'", uid, endlessLeague.leagueId, endlessLeague.groupId);
                var playerCommand = new MySqlCommand(playerSql, conn);
                var playerReader = playerCommand.ExecuteReader();

                while (playerReader.Read())
                {
                    playerState.rankingLeagueGroup = Convert.ToInt64(playerReader["ranking_league_group"]);
                    playerState.rankingLeague = Convert.ToInt64(playerReader["ranking_league"]);
                    var rank = Convert.ToInt64(playerReader["league_rank"]);
                    if (rank <= endlessLeague.numUp)
                    {
                        endlessLeague.leagueId += 1;

                        switch (endlessLeague.leagueId)
                        {
                            case 3:
                            case 6:
                            case 9:
                            case 12:
                            case 15:
                            case 18:
                            case 19:
                            case 20:
                                var getShahraSql = Db.GetCommand("SELECT * FROM `sw_chao` WHERE id = '{0}'", Chao.ChaoID.Shahra);
                                var getShahraCommand = new MySqlCommand(getShahraSql, conn);
                                var getShahraReader = getShahraCommand.ExecuteReader();

                                getShahraReader.Read();
                                Chao chao = new();
                                chao.chaoID = Convert.ToString(getShahraReader["id"]);

                                var populateChaoStateStatus = Chao.PopulateChaoState(conn, uid, out Chao[] chaoState);
                                var getChaoIndex = Chao.FindChaoInChaoState(Convert.ToInt32(chao.chaoID), chaoState);
                                if (chaoState[getChaoIndex].status == (sbyte)Chao.Status.NotOwned || chaoState[getChaoIndex].level < 10)
                                {
                                    var chaoPrize = ChaoSpinPrize.ChaoToChaoSpinPrize(chao);
                                }
                                else if (chaoState[getChaoIndex].status == (sbyte)Chao.Status.MaxLevel)
                                {
                                    var itemPrize = new Item((long)Item.ItemID.SpecialEgg, 1);
                                }
                                getShahraReader.Close();
                                break;
                            default:
                                break;
                        }
                    }
                    else if (endlessLeague.leagueId == 20)
                    {
                        var getShahraSql = Db.GetCommand("SELECT * FROM `sw_chao` WHERE id = '{0}'", Chao.ChaoID.Shahra);
                        var getShahraCommand = new MySqlCommand(getShahraSql, conn);
                        var getShahraReader = getShahraCommand.ExecuteReader();

                        getShahraReader.Read();
                        Chao chao = new();
                        chao.chaoID = Convert.ToString(getShahraReader["id"]);

                        var populateChaoStateStatus = Chao.PopulateChaoState(conn, uid, out Chao[] chaoState);
                        var getChaoIndex = Chao.FindChaoInChaoState(Convert.ToInt32(chao.chaoID), chaoState);
                        if (chaoState[getChaoIndex].status == (sbyte)Chao.Status.NotOwned || chaoState[getChaoIndex].level < 10)
                        {
                            var chaoPrize = ChaoSpinPrize.ChaoToChaoSpinPrize(chao);
                        }
                        else if (chaoState[getChaoIndex].status == (sbyte)Chao.Status.MaxLevel)
                        {
                            var itemPrize = new Item((long)Item.ItemID.SpecialEgg, 1);
                        }
                    }
                    else if (endlessLeague.leagueId >= 9 && rank > (endlessLeague.numGroupMember - endlessLeague.numDown))
                    {
                        endlessLeague.leagueId -= 1;
                    }
                    playerState.rankingLeague = endlessLeague.leagueId;
                }

                playerReader.Close();
            }

            var generateEndlessLeagueStatus = GenerateEndlessLeagueData(conn, uid, out LeagueData currentEndlessLeague);
            if (generateEndlessLeagueStatus != SRStatusCode.Ok)
            {
                return generateEndlessLeagueStatus;
            }

            var updateGroupLeagueIdSql = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_players WHERE ranking_league = '{0}' AND ranking_league_group = '{1}'", playerState.rankingLeague, playerState.rankingLeagueGroup);
            var countLeaguePlayersInGroup = Convert.ToInt64(updateGroupLeagueIdSql);

            if (countLeaguePlayersInGroup == currentEndlessLeague.numGroupMember)
            {
                currentEndlessLeague.groupId += 1;
                var updateCurrentEndlessLeagueSql = Db.GetCommand(@"UPDATE `sw_endlessleaguedata` SET group_id = '{0}' WHERE league_id = '{1}'", currentEndlessLeague.groupId, currentEndlessLeague.leagueId);
                var updateCurrentEndlessLeagueCommand = new MySqlCommand(updateCurrentEndlessLeagueSql, conn);
                updateCurrentEndlessLeagueCommand.ExecuteNonQuery();
            }

            var updatePlayerStateSql = Db.GetCommand(@"UPDATE `sw_players` SET ranking_league = '{0}', group_id = '{1}' WHERE id= '{2}'",  playerState.rankingLeague, currentEndlessLeague.groupId, uid);
            var updatePlayerStateCommand = new MySqlCommand(updatePlayerStateSql, conn);
            int rowsAffected = updatePlayerStateCommand.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                // Failed to find row with this user ID
                return SRStatusCode.MissingPlayer;
            }

            conn.Close();

            SaveEndlessLeagueData(conn, uid, ref currentEndlessLeague);
            return SRStatusCode.Ok;
        }
        public static SRStatusCode CalculateQuickRunnersLeague(MySqlConnection conn, string uid)
        {

            PlayerState playerState = new();
            var populateStatus = playerState.Populate(conn, uid);
            if (populateStatus != SRStatusCode.Ok)
            {
                return populateStatus;
            }

            DateTimeOffset leagueReset = new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                0, 0, 0, 0).AddDays(7);

            if (DateTime.Now >= leagueReset)
            {
                LeagueData quickLeague = new();
                var playerSql = Db.GetCommand(@"SELECT *, rank() OVER (PARTITION BY quick_league_rank ORDER BY quick_ranking_league_group, quick_league_high_score DESC) AS quick_league_rank FROM `sw_players` WHERE id='{0}' AND quick_ranking_league='{1}' AND quick_ranking_league_group = '{2}'", uid, quickLeague.leagueId, quickLeague.groupId);
                var playerCommand = new MySqlCommand(playerSql, conn);
                var playerReader = playerCommand.ExecuteReader();

                if (playerReader.HasRows)
                {
                    playerState.quickRankingLeagueGroup = Convert.ToInt64(playerReader["quick_ranking_league_group"]);
                    playerState.quickRankingLeague = Convert.ToInt64(playerReader["quick_ranking_league"]);
                    var quickLeagueRank = Convert.ToInt64(playerReader["quick_league_rank"]);
                    if (quickLeagueRank <= quickLeague.numUp)
                    {
                        quickLeague.leagueId += 1;
                        switch (quickLeague.leagueId)
                        {
                            case 3:
                            case 6:
                            case 9:
                            case 12:
                            case 15:
                            case 18:
                            case 19:
                            case 20:
                                var getDarkQueenSql = Db.GetCommand("SELECT * FROM `sw_chao` WHERE id = '{0}'", Chao.ChaoID.DarkQueen);
                                var getDarkQueenCommand = new MySqlCommand(getDarkQueenSql, conn);
                                var getDarkQueenReader = getDarkQueenCommand.ExecuteReader();

                                getDarkQueenReader.Read();
                                Chao chao = new();
                                chao.chaoID = Convert.ToString(getDarkQueenReader["id"]);

                                var populateChaoState = Chao.PopulateChaoState(conn, uid, out Chao[] chaoState);
                                var getChaoIndex = Chao.FindChaoInChaoState(Convert.ToInt32(chao.chaoID), chaoState);
                                if (chaoState[getChaoIndex].status == (sbyte)Chao.Status.NotOwned || chaoState[getChaoIndex].level < 10)
                                {
                                    var chaoPrize = ChaoSpinPrize.ChaoToChaoSpinPrize(chao);
                                }
                                else if (chaoState[getChaoIndex].status == (sbyte)Chao.Status.MaxLevel)
                                {
                                    var itemPrize = new Item((long)Item.ItemID.SpecialEgg, 1);
                                }
                                getDarkQueenReader.Close();
                                break;
                            default:
                                break;
                        }
                    }
                    else if (quickLeague.leagueId == 20)
                    {
                        var getDarkQueenSql = Db.GetCommand("SELECT * FROM `sw_chao` WHERE id = '{0}'", Chao.ChaoID.DarkQueen);
                        var getDarkQueenCommand = new MySqlCommand(getDarkQueenSql, conn);
                        var getDarkQueenReader = getDarkQueenCommand.ExecuteReader();

                        getDarkQueenReader.Read();
                        Chao chao = new();
                        chao.chaoID = Convert.ToString(getDarkQueenReader["id"]);

                        var populateChaoState = Chao.PopulateChaoState(conn, uid, out Chao[] chaoState);
                        chao.chaoID = Convert.ToString(Chao.ChaoID.DarkQueen);
                        var getChaoIndex = Chao.FindChaoInChaoState(Convert.ToInt32(chao.chaoID), chaoState);
                        if (chaoState[getChaoIndex].status == (sbyte)Chao.Status.NotOwned || chaoState[getChaoIndex].level < 10)
                        {
                            var chaoPrize = ChaoSpinPrize.ChaoToChaoSpinPrize(chao);
                        }
                        else if (chaoState[getChaoIndex].status == (sbyte)Chao.Status.MaxLevel)
                        {
                            var itemPrize = new Item((long)Item.ItemID.SpecialEgg, 1);
                        }
                    }
                    else if (quickLeague.leagueId >= 9 && quickLeagueRank > (quickLeague.numGroupMember - quickLeague.numDown))
                    {
                        quickLeague.leagueId -= 1;
                    }
                    playerState.quickRankingLeague = quickLeague.leagueId;
                }

                conn.Close();
            }

            var generateQuickLeagueStatus = GenerateQuickLeagueData(conn, uid, out LeagueData currentQuickLeague);
            if (generateQuickLeagueStatus != SRStatusCode.Ok)
            {
                return generateQuickLeagueStatus;
            }

            var updateGroupLeagueIdSql = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_players WHERE quick_ranking_league = '{0}' AND quick_ranking_league_group = '{1}'", playerState.quickRankingLeague, playerState.quickRankingLeagueGroup);
            var countLeaguePlayersInGroup = Convert.ToInt64(updateGroupLeagueIdSql);

            if (countLeaguePlayersInGroup == currentQuickLeague.numGroupMember)
            {
                currentQuickLeague.groupId += 1;
                var updatecurrentQuickLeagueSql = Db.GetCommand(@"UPDATE `sw_quickleaguedata` SET group_id = '{0}' WHERE league_id = '{1}'", currentQuickLeague.groupId, currentQuickLeague.leagueId);
                var updatecurrentQuickLeagueCommand = new MySqlCommand(updatecurrentQuickLeagueSql, conn);
                updatecurrentQuickLeagueCommand.ExecuteNonQuery();
            }

            var updatePlayerStateSql = Db.GetCommand(@"UPDATE `sw_players` SET quick_ranking_league = '{0}', quick_ranking_league_group = {1}' WHERE id= '{2}'", currentQuickLeague.leagueId, currentQuickLeague.groupId, uid);
            var updatePlayerStateCommand = new MySqlCommand(updatePlayerStateSql, conn);
            int rowsAffected = updatePlayerStateCommand.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                // Failed to find row with this user ID
                return SRStatusCode.MissingPlayer;
            }

            SaveQuickLeagueData(conn, uid, ref currentQuickLeague);
            return SRStatusCode.Ok;
        }

        public static SRStatusCode ClearLeagueScoresData(MySqlConnection conn)
        {
            var resetLeagueScoresSql = Db.GetCommand(@"UPDATE `sw_players` SET league_high_score = '{0}', quick_league_high_score = '{1}', total_score = '{2}', quick_total_score = '{3}'", 0, 0, 0, 0);
            var resetLeagueScoresCommand = new MySqlCommand(resetLeagueScoresSql, conn);
            resetLeagueScoresCommand.ExecuteNonQuery();
            return SRStatusCode.Ok;
        }

        //public LeagueData()
        //{
        //    leagueId = "0";
        //    groupId = "0";
        //    numUp = "40";
        //    numDown = "0";
        //    numGroupMember = "0";
        //    highScoreOpe = Array.Empty<OperatorScore>();
        //    totalScoreOpe = Array.Empty<OperatorScore>();
        //}
    }
}
