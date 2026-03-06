namespace CardRPG.Core.Models;

public enum CardType
{
    Attack,
    Defense,
    Power
}

public enum CardAbility
{
    None,
    Burn,
    Poison,
    Lifesteal,
    DoubleStrike,
    Stun,
    Weaken,
    Fortify,
    Draw,
    ManaSurge,
    Bleed,
    Thorns
}

public enum CardRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

public class Card
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Cost { get; set; }
    public CardType Type { get; set; }
    public int Value { get; set; }
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int CooldownTurns { get; set; } = 0;
    public bool IsReady => CooldownTurns == 0;
    public CardAbility Ability { get; set; } = CardAbility.None;
    public CardRarity Rarity { get; set; } = CardRarity.Common;
    public bool IsPassive { get; set; } = false;
    public string PassiveEffect { get; set; } = "";
    public int PassiveValue { get; set; } = 0;
    public int UpgradeLevel { get; set; } = 0;
    public bool IsCurse { get; set; } = false;
    public bool IsEvolved { get; set; } = false;
    public string BaseName { get; set; } = "";

    public string DisplayName => Name;

    public int UpgradeCost => GetUpgradeCost();

    public int RemoveCost => Rarity switch
    {
        CardRarity.Common => 10,
        CardRarity.Uncommon => 25,
        CardRarity.Rare => 50,
        CardRarity.Legendary => 150,
        _ => 10
    };

    public int EvolveCost => Rarity switch
    {
        CardRarity.Common => 150,
        CardRarity.Uncommon => 300,
        CardRarity.Rare => 600,
        _ => 9999
    };

    public bool CanEvolve => !IsEvolved && UpgradeLevel >= 2
        && CardEvolution.GetEvolution(BaseName.Length > 0 ? BaseName : Name) != null;

    public Card() { }

    public Card(string name, int cost, CardType type, int value, string description = null,
                CardAbility ability = CardAbility.None, CardRarity rarity = CardRarity.Common)
    {
        Name = name;
        BaseName = name;
        Cost = cost;
        Type = type;
        Value = value;
        Ability = ability;
        Rarity = rarity;
        Description = description ?? GenerateDescription();
    }

    private string GenerateDescription()
    {
        string baseDesc = Type switch
        {
            CardType.Attack => $"Deal {Value} damage.",
            CardType.Defense => $"Gain {Value} armor.",
            CardType.Power => $"Special effect ({Value}).",
            _ => ""
        };

        string abilityDesc = Ability switch
        {
            CardAbility.Burn => " Burns for 3 dmg/turn for 2 turns.",
            CardAbility.Poison => " Poisons for 2 dmg/turn for 3 turns.",
            CardAbility.Lifesteal => " Heals you for 50% of damage dealt.",
            CardAbility.DoubleStrike => " Hits twice.",
            CardAbility.Stun => " Enemy skips next attack.",
            CardAbility.Weaken => " Reduces enemy damage by 3.",
            CardAbility.Fortify => " Armor persists next turn.",
            CardAbility.Draw => " Draw 1 extra card next turn.",
            CardAbility.ManaSurge => " Restores 1 mana.",
            CardAbility.Bleed => " Bleeds 2 dmg/turn for 3 turns.",
            CardAbility.Thorns => " Reflects 3 damage when hit.",
            _ => ""
        };

        return baseDesc + abilityDesc;
    }

    public Card Clone()
    {
        return new Card(Name, Cost, Type, Value, Description, Ability, Rarity)
        {
            IsPassive = IsPassive,
            PassiveEffect = PassiveEffect,
            PassiveValue = PassiveValue,
            UpgradeLevel = UpgradeLevel,
            IsCurse = IsCurse,
            IsEvolved = IsEvolved,
            BaseName = BaseName
        };
    }

    public void Evolve()
    {
        var evo = CardEvolution.GetEvolution(BaseName.Length > 0 ? BaseName : Name);
        if (evo == null) return;
        Name = evo.EvolvedName;
        Value += evo.BonusValue;
        if (evo.GrantAbility.HasValue)
            Ability = evo.GrantAbility.Value;
        IsEvolved = true;
        Description = GenerateDescription();
    }

    public int GetUpgradeCost()
    {
        int baseCost = Rarity switch
        {
            CardRarity.Common => 15,
            CardRarity.Uncommon => 30,
            CardRarity.Rare => 60,
            CardRarity.Legendary => 120,
            _ => 15
        };
        return baseCost + (UpgradeLevel * 10);
    }

    public int MaxUpgradeLevel => 5;

    public bool CanUpgrade => UpgradeLevel < MaxUpgradeLevel;

    public void Upgrade()
    {
        if (!CanUpgrade) return;
        UpgradeLevel++;
        Value += 1;

        // Rarity promotion at certain upgrade levels
        if (UpgradeLevel == 3 && Rarity == CardRarity.Common)
            Rarity = CardRarity.Uncommon;
        else if (UpgradeLevel == 5 && Rarity == CardRarity.Uncommon)
            Rarity = CardRarity.Rare;

        // Update display name with upgrade indicator
        if (!Name.Contains('+'))
            Name = $"{Name} +{UpgradeLevel}";
        else
            Name = Name[..Name.LastIndexOf('+')] + $"+{UpgradeLevel}";

        Description = GenerateDescription();
    }

    public int GetSellPrice()
    {
        return Rarity switch
        {
            CardRarity.Common => 5,
            CardRarity.Uncommon => 15,
            CardRarity.Rare => 40,
            CardRarity.Legendary => 100,
            _ => 5
        };
    }
}