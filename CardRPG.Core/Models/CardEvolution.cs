namespace CardRPG.Core.Models;

public class CardEvolution
{
    public string BaseName { get; set; } = "";
    public string EvolvedName { get; set; } = "";
    public string EvolvedDescription { get; set; } = "";
    public int BonusValue { get; set; } = 0;
    public int HitCount { get; set; } = 1;
    public CardAbility? GrantAbility { get; set; }

    private static readonly List<CardEvolution> _definitions = new()
    {
        new() { BaseName = "Strike",       EvolvedName = "Power Strike",    EvolvedDescription = "Deal {value} damage with crushing force.",  BonusValue = 8,  GrantAbility = CardAbility.Bleed },
        new() { BaseName = "Defend",       EvolvedName = "Fortress",        EvolvedDescription = "Gain {value} armor and reflect attacks.",    BonusValue = 6,  GrantAbility = CardAbility.Thorns },
        new() { BaseName = "Quick Slash",  EvolvedName = "Whirlwind",       EvolvedDescription = "Strike twice for {value} damage each.",      BonusValue = 4,  HitCount = 2, GrantAbility = CardAbility.DoubleStrike },
        new() { BaseName = "Heal",         EvolvedName = "Regenerate",      EvolvedDescription = "Restore {value} HP and mana.",               BonusValue = 8,  GrantAbility = CardAbility.ManaSurge },
        new() { BaseName = "Heavy Blow",   EvolvedName = "Crushing Strike", EvolvedDescription = "Deal {value} damage and stun the enemy.",    BonusValue = 10, GrantAbility = CardAbility.Stun },
        new() { BaseName = "Flame Strike", EvolvedName = "Inferno",         EvolvedDescription = "Deal {value} blazing damage.",               BonusValue = 12, GrantAbility = CardAbility.Burn },
        new() { BaseName = "Poison Dart",  EvolvedName = "Toxic Volley",    EvolvedDescription = "Poison for {value} damage per turn.",        BonusValue = 5,  HitCount = 2, GrantAbility = CardAbility.Poison },
        new() { BaseName = "Iron Guard",   EvolvedName = "Titanwall",       EvolvedDescription = "Gain {value} armor with thorns.",            BonusValue = 8,  GrantAbility = CardAbility.Thorns },
        new() { BaseName = "Dragon Breath",EvolvedName = "Dragonfire",      EvolvedDescription = "Deal {value} fire damage in a wide arc.",    BonusValue = 15, GrantAbility = CardAbility.Burn },
        new() { BaseName = "Soul Drain",   EvolvedName = "Soul Harvest",    EvolvedDescription = "Drain {value} HP from the enemy.",           BonusValue = 10, GrantAbility = CardAbility.Lifesteal },
    };

    public static CardEvolution? GetEvolution(string baseName)
        => _definitions.FirstOrDefault(e => e.BaseName.Equals(baseName, StringComparison.OrdinalIgnoreCase));
}
