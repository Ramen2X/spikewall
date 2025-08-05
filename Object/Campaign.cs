namespace spikewall.Object
{
    public class Campaign
    {
        public long? campaignType { get; set; }
        public long? campaignContent { get; set; }
        public long? campaignSubContent { get; set; }
        public long? campaignStartTime { get; set; }
        public long? campaignEndTime { get; set; }

        /// <summary>
        /// Enum that contains the names 
        /// and IDs of all Campaign Types.
        /// </summary>
        public enum CampaignType
        {
            BankedRingBonus,
            DailyMissionBonus,
            ChaoRouletteCost,
            GameItemCost,
            CharacterUpgradeCost,
            PurchaseAddRings,
            JackpotValueBonus,
            MileagePassingRingBonus,
            PurchaseAddEnergies,
            PurchaseAddRedRings,
            PurchaseAddRedRingsNoChargeUser,
            SendAddEnergies,
            InviteCount,
            PremiumRouletteOdds,
            FreeWheelSpinCount,
            ContinueCost,
            PurchaseAddRaidEnergies
        }

        public enum CollectEventType
        {
            GetAnimals,
            GetRing,
            RunDistance
        }

        public enum AdvertEventType
        {
            Roulette,
            Character,
            Shop
        }

        public enum EventID
        {
            SpecialStage,
            RaidBoss,
            CollectObject,
            Gacha,
            Advert,
            Quick,
            BGM
        }
    }
}
