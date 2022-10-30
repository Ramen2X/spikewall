namespace spikewall.Object
{
    public class MapInfo
    {
        public long? mapDistance { get; set; }
        public long? numBossAttack { get; set; }
        public long? stageDistance { get; set; }
        public long? stageMaxScore { get; set; }

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

        public MapInfo(long p_mapDistance, long p_numBossAttack, long p_stageDistance, long p_stageMaxScore)
        {
            mapDistance = p_mapDistance;
            numBossAttack = p_numBossAttack;
            stageDistance = p_stageDistance;
            stageMaxScore = p_stageMaxScore;
        }
    }

    public class MileageMapState : MapInfo
    {
        public long? episode { get; set; }
        public long? chapter { get; set; }
        public long? point { get; set; }
        public long? stageTotalScore { get; set; }
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
            long p_stageMaxScore, long p_episode, long p_chapter, long p_point, long p_stageTotalScore)
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

        public void AdvanceToNextChapter()
        {
            stageTotalScore = 0;
            // Sonic Runners' story ends at Chapter 50,
            // make sure to not progress further.
            if (chapter < 50) {
                chapter += 1;
            }
            episode = 1;
            point = 0;
            chapterStartTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public void AddScore(long p_score)
        {
            stageTotalScore += p_score;
        }
    }

    public class MileageFriend
    {
        public string? friendId { get; set; }
        public string? name { get; set; }
        public string? url { get; set; }
        public MileageMapState? mapState { get; set; }
    }
}
