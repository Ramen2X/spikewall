namespace spikewall.Object
{
    public class RouletteInfo
    {
        public long? LoginRouletteID { get; set; }
        public long? RoulettePeriodEnd { get; set; }
        public long? RouletteCountInPeriod { get; set; }
        public bool? GotJackpotThisPeriod { get; set; }
    }
}
