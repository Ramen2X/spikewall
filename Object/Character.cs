using System.Text.Json.Serialization;

namespace spikewall.Object
{
    public class Character
    {
        // The internal ID for this character.
        public string? characterId { get; set; }

        // The amount of rings this character
        // currently costs to level up.
        public ulong? numRings { get; set; }

        // UNUSED: The amount of Red Star Rings
        // this character costs to level up.
        public ulong? numRedRings { get; set; }

        // The amount of rings this
        // character costs to buy/limit smash.
        public ulong? priceNumRings { get; set; }

        // The amount of Red Star Rings this
        // character costs to buy/limit smash.
        public ulong? priceNumRedRings { get; set; }

        // Whether or not the character is unlocked.
        public sbyte? status { get; set; }

        // The level of the character.
        public sbyte? level { get; set; }

        // How many rings until
        // the next level up??
        public ulong? exp { get; set; }

        // Amount of times the character
        // has been limit smashed.
        public sbyte? star { get; set; }

        // Maximum amount of times the
        // character can be limit smashed.
        public sbyte? starMax { get; set; }

        // How the character can be unlocked
        // (Purchasable with Red Rings, Winnable
        // on the Premium Roulette, etc).
        // TODO: Probably move this to an enum soon.
        public sbyte? lockCondition { get; set; }

        // Not sure what this is right now.
        public Campaign[]? campaignList { get; set; }

        // The current levels for each ability.
        public long[]? abilityLevel { get; set; }

        // Apparently this may be unused?
        // Otherwise, not sure what this is.
        public long[]? abilityNumRings { get; set; }

        // The current ability to be leveled up?
        public long[]? abilityLevelup { get; set; }

        // This is not always sent, so it needs to be
        // specifically handled during deserialization.
        // I'm also not sure what this is right now.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long[]? abilityLevelupExp { get; set; }
    }
}
