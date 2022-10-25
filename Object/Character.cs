using System.Text.Json.Serialization;

namespace spikewall.Object
{
    public class CharacterBase
    {
        // The internal ID for this character.
        public string? characterId { get; set; }

        // The amount of rings this
        // character costs to buy/level up?
        public Int64? numRings { get; set; }

        // The amount of Red Star Rings
        // this character costs to buy?
        public Int64? numRedRings { get; set; }

        // The amount of rings this
        // character costs to limit break.
        public Int64? priceNumRings { get; set; }

        // Possibly unused? Unsure right now.
        public Int64? priceNumRedRings { get; set; }
    }

    // TODO: Do these actually need to be separate
    // classes? This is how they're set up in Outrun,
    // but fact checking would not hurt.
    public class Character : CharacterBase
    {
        // Not sure what this is right now.
        public Int64? status { get; set; }

        // The level of the character.
        public Int64? level { get; set; }

        // How many rings until
        // the next level up??
        public Int64? exp { get; set; }

        // Amount of times the character
        // has been limit broken.
        public Int64? star { get; set; }

        // Maximum amount of times the
        // character can be limit broken.
        public Int64? starMax { get; set; }

        // How the character can be unlocked
        // (Purchasable with Red Rings, Winnable
        // on the Premium Roulette, etc).
        public Int64? lockCondition { get; set; }

        // Not sure what this is right now.
        public Campaign[]? campaignList { get; set; }

        // The current levels for each ability.
        public Int64[]? abilityLevel { get; set; }

        // Apparently this may be unused?
        // Otherwise, not sure what this is.
        public Int64[]? abilityNumRings { get; set; }

        // Not sure what this is either.
        public Int64[]? abilityLevelup { get; set; }

        // This is not always sent, so it needs to be
        // specifically handled during deserialization.
        // I'm also not sure what this is right now.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Int64[]? abilityLevelupExp { get; set; }
    }
}
