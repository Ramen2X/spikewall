using MySql.Data.MySqlClient;
using spikewall.Response;
using System.Text.Json.Serialization;

namespace spikewall.Object
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class MapInfo
    {
        public long? mapDistance { get; set; }
        public long? numBossAttack { get; set; }
        public long? stageDistance { get; set; }
        public ulong? stageMaxScore { get; set; }

        public MapInfo()
        {
            // TODO: These are hardcoded purely to make the default
            // constructors of any classes derived from it happy.
            // Find a better method of doing this.
            mapDistance = 0;
            numBossAttack = 0;
            stageDistance = 4201337;
            stageMaxScore = 15110182021018;
        }

        public MapInfo(long p_mapDistance, long p_numBossAttack, long p_stageDistance, ulong p_stageMaxScore)
        {
            mapDistance = p_mapDistance;
            numBossAttack = p_numBossAttack;
            stageDistance = p_stageDistance;
            stageMaxScore = p_stageMaxScore;
        }
    }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class MileageMapState : MapInfo
    {
        public sbyte? episode { get; set; }
        public sbyte? chapter { get; set; }
        public long? point { get; set; }
        public ulong? stageTotalScore { get; set; }
        public long? chapterStartTime { get; set; }

        // TODO: Although a new account should always start at Chapter 1
        // Episode 1, it probably wouldn't hurt to make this configurable.
        public MileageMapState()
        {
            episode = 1;
            chapter = 1;
            point = 0;
            stageTotalScore = 0;
            chapterStartTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public MileageMapState(long p_mapDistance, long p_numBossAttack, long p_stageDistance,
            ulong p_stageMaxScore, sbyte p_episode, sbyte p_chapter, long p_point, ulong p_stageTotalScore)
        {
            mapDistance = p_mapDistance;
            numBossAttack = p_numBossAttack;
            stageDistance = p_stageDistance;
            stageMaxScore = p_stageMaxScore;
            episode = p_episode;
            chapter = p_chapter;
            point = p_point;
            stageTotalScore = p_stageTotalScore;
            chapterStartTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public void AdvanceToNextEpisode()
        {
            stageTotalScore = 0;
            // Sonic Runners' story ends at Episode 50,
            // make sure to not progress further.
            if (episode < 50) {
                episode += 1;
            }
            chapter = 1;
            point = 0;
            chapterStartTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public void AddScore(ulong p_score)
        {
            stageTotalScore += p_score;
        }

        public SRStatusCode Populate(MySqlConnection conn, string uid)
        {
            var sql = Db.GetCommand(@"SELECT * FROM `sw_mileagemapstates` WHERE user_id = '{0}';", uid);
            var command = new MySqlCommand(sql, conn);

            var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                // Somehow failed to find player, return error code
                return SRStatusCode.MissingPlayer;
            }

            reader.Read();
            this.chapter = reader.GetSByte("chapter");
            this.episode = reader.GetSByte("episode");
            this.point = reader.GetInt64("point");
            this.stageTotalScore = reader.GetUInt64("stage_total_score");
            this.chapterStartTime = reader.GetInt64("chapter_start_time");

            this.mapDistance = reader.GetInt64("map_distance");
            this.numBossAttack = reader.GetInt64("num_boss_attack");
            this.stageDistance = reader.GetInt64("stage_distance");
            this.stageMaxScore = reader.GetUInt64("stage_max_score");

            reader.Close();

            return SRStatusCode.Ok;
        }

        public SRStatusCode Save(MySqlConnection conn, string uid)
        {
            var sql = Db.GetCommand(
                @"UPDATE `sw_mileagemapstates` SET
                    episode = '{0}',
                    chapter = '{1}',
                    point = '{2}',
                    stage_total_score = '{3}',
                    chapter_start_time = '{4}',
                    map_distance = '{5}',
                    num_boss_attack = '{6}',
                    stage_distance = '{7}',
                    stage_max_score = '{8}'
                  WHERE user_id = '{9}';",
                    this.episode,
                    this.chapter,
                    this.point,
                    this.stageTotalScore,
                    this.chapterStartTime,
                    this.mapDistance,
                    this.numBossAttack,
                    this.stageDistance,
                    this.stageMaxScore,
                    uid);
            var command = new MySqlCommand(sql, conn);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                // Failed to find row with this user ID
                return SRStatusCode.MissingPlayer;
            }

            return SRStatusCode.Ok;
        }
    }

    public class MileageFriend
    {
        public string? friendId { get; set; }
        public string? name { get; set; }
        public string? url { get; set; }
        public MileageMapState? mapState { get; set; }
    }

    public class MileageReward
    {
        public long? type { get; set; }
        public long? itemId { get; set; }
        public long? numItem { get; set; }
        public sbyte? point { get; set; }
        public long? limitTime { get; set; }
    }

    public class MileageIncentive
    {
        public long? type { get; set; }
        public long? itemId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? friendId { get; set; }
        public long? numItem { get; set; }
        public long? pointId { get; set; }
    }
}
