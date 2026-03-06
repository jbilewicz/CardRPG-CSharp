namespace CardRPG.Core.Models;

public static class CardLibrary
{
    public static Card Strike => new("Strike", 1, CardType.Attack, 6, rarity: CardRarity.Common);
    public static Card Defend => new("Defend", 1, CardType.Defense, 5, rarity: CardRarity.Common);
    public static Card QuickSlash => new("Quick Slash", 1, CardType.Attack, 4, "A swift blade strike.", rarity: CardRarity.Common);
    public static Card Heal => new("Heal", 1, CardType.Power, 12, "Restore 12 HP.", rarity: CardRarity.Common);
    public static Card HeavyBlow => new("Heavy Blow", 2, CardType.Attack, 14, rarity: CardRarity.Common);
    public static Card IronGuard => new("Iron Guard", 2, CardType.Defense, 12, rarity: CardRarity.Common);
    public static Card ArcaneTap => new("Arcane Tap", 0, CardType.Power, 1, "Restore 1 mana.", CardAbility.ManaSurge, CardRarity.Common);

    public static Card FlameStrike => new("Flame Strike", 2, CardType.Attack, 10, ability: CardAbility.Burn, rarity: CardRarity.Uncommon);
    public static Card PoisonDart => new("Poison Dart", 1, CardType.Attack, 3, ability: CardAbility.Poison, rarity: CardRarity.Uncommon);
    public static Card VampiricSlash => new("Vampiric Slash", 2, CardType.Attack, 8, ability: CardAbility.Lifesteal, rarity: CardRarity.Uncommon);
    public static Card TwinStrike => new("Twin Strike", 2, CardType.Attack, 6, ability: CardAbility.DoubleStrike, rarity: CardRarity.Uncommon);
    public static Card ShieldBash => new("Shield Bash", 2, CardType.Defense, 8, ability: CardAbility.Stun, rarity: CardRarity.Uncommon);
    public static Card WeakeningCurse => new("Weakening Curse", 1, CardType.Power, 0, ability: CardAbility.Weaken, rarity: CardRarity.Uncommon);
    public static Card Fortification => new("Fortification", 2, CardType.Defense, 10, ability: CardAbility.Fortify, rarity: CardRarity.Uncommon);
    public static Card BattleTrance => new("Battle Trance", 1, CardType.Power, 0, ability: CardAbility.Draw, rarity: CardRarity.Uncommon);
    public static Card Meditation => new("Meditation", 0, CardType.Power, 2, "Restore 2 mana.", CardAbility.ManaSurge, CardRarity.Uncommon);

    public static Card DragonBreath => new("Dragon Breath", 4, CardType.Attack, 40, ability: CardAbility.Burn, rarity: CardRarity.Rare);
    public static Card SoulDrain => new("Soul Drain", 3, CardType.Attack, 20, ability: CardAbility.Lifesteal, rarity: CardRarity.Rare);
    public static Card ThornWall => new("Thorn Wall", 3, CardType.Defense, 18, ability: CardAbility.Thorns, rarity: CardRarity.Rare);
    public static Card ArcaneBarrage => new("Arcane Barrage", 3, CardType.Attack, 15, ability: CardAbility.DoubleStrike, rarity: CardRarity.Rare);
    public static Card ManaCrystal => new("Mana Crystal", 0, CardType.Power, 2, ability: CardAbility.ManaSurge, rarity: CardRarity.Rare);
    public static Card ManaSpring => new("Mana Spring", 0, CardType.Power, 3, "Restore 3 mana.", CardAbility.ManaSurge, CardRarity.Rare);

    public static Card Oblivion => new("Oblivion", 5, CardType.Attack, 60, ability: CardAbility.Burn, rarity: CardRarity.Legendary);
    public static Card DemonicPact => new("Demonic Pact", 3, CardType.Attack, 35, ability: CardAbility.Lifesteal, rarity: CardRarity.Legendary);

    public static Card IronHeart => new("Iron Heart", 0, CardType.Power, 0, "PASSIVE: +10 Max HP", rarity: CardRarity.Uncommon)
        { IsPassive = true, PassiveEffect = "MaxHP", PassiveValue = 10 };
    public static Card ManaWell => new("Mana Well", 0, CardType.Power, 0, "PASSIVE: +1 Max Mana", rarity: CardRarity.Rare)
        { IsPassive = true, PassiveEffect = "MaxMana", PassiveValue = 1 };
    public static Card BloodRune => new("Blood Rune", 0, CardType.Power, 0, "PASSIVE: +3 DMG", rarity: CardRarity.Uncommon)
        { IsPassive = true, PassiveEffect = "Damage", PassiveValue = 3 };
    public static Card LuckyCharm => new("Lucky Charm", 0, CardType.Power, 0, "PASSIVE: +5% Crit", rarity: CardRarity.Rare)
        { IsPassive = true, PassiveEffect = "CritChance", PassiveValue = 5 };
    public static Card DragonHeart => new("Dragon Heart", 0, CardType.Power, 0, "PASSIVE: +25 Max HP", rarity: CardRarity.Legendary)
        { IsPassive = true, PassiveEffect = "MaxHP", PassiveValue = 25 };
    public static Card ArcaneCore => new("Arcane Core", 0, CardType.Power, 0, "PASSIVE: +2 Max Mana", rarity: CardRarity.Legendary)
        { IsPassive = true, PassiveEffect = "MaxMana", PassiveValue = 2 };

    public static List<Card> GetStarterDeck() => new()
    {
        Strike.Clone(), Strike.Clone(),
        Defend.Clone(), Defend.Clone(),
        QuickSlash.Clone(),
        Heal.Clone(),
        ArcaneTap.Clone(),
    };

    public static List<Card> GetCommonPool() => new()
    {
        Strike, Defend, QuickSlash, Heal, HeavyBlow, IronGuard, ArcaneTap,
    };

    public static List<Card> GetUncommonPool() => new()
    {
        FlameStrike, PoisonDart, VampiricSlash, TwinStrike,
        ShieldBash, WeakeningCurse, Fortification, BattleTrance, Meditation,
        IronHeart, BloodRune,
    };

    public static List<Card> GetRarePool() => new()
    {
        DragonBreath, SoulDrain, ThornWall, ArcaneBarrage, ManaCrystal, ManaSpring,
        ManaWell, LuckyCharm,
    };

    public static List<Card> GetLegendaryPool() => new()
    {
        Oblivion, DemonicPact, DragonHeart, ArcaneCore,
    };

    public static Card GetRandomCard(Random rng, int playerLevel)
    {
        int roll = rng.Next(100);
        List<Card> pool;

        if (playerLevel >= 8 && roll < 5)
            pool = GetLegendaryPool();
        else if (playerLevel >= 5 && roll < 20)
            pool = GetRarePool();
        else if (playerLevel >= 2 && roll < 50)
            pool = GetUncommonPool();
        else
            pool = GetCommonPool();

        return pool[rng.Next(pool.Count)].Clone();
    }

    public static Card GetBossDropCard(Random rng, int realmId)
    {
        var bossPool = realmId switch
        {
            <= 3 => GetUncommonPool(),
            <= 6 => GetRarePool(),
            <= 8 => GetRarePool().Concat(GetLegendaryPool()).ToList(),
            _ => GetLegendaryPool(),
        };

        return bossPool[rng.Next(bossPool.Count)].Clone();
    }

    public static Card? TryCraft(Card a, Card b)
    {
        if (a.Name != b.Name) return null;
        if (a.IsPassive) return null;

        CardRarity newRarity = a.Rarity switch
        {
            CardRarity.Common => CardRarity.Uncommon,
            CardRarity.Uncommon => CardRarity.Rare,
            _ => a.Rarity
        };

        int valueBoost = a.Rarity switch
        {
            CardRarity.Common => (int)(a.Value * 0.6),
            CardRarity.Uncommon => (int)(a.Value * 0.5),
            _ => 0
        };

        if (newRarity == a.Rarity) return null;

        string newName = a.Rarity == CardRarity.Common ? $"Enhanced {a.Name}" : $"Superior {a.Name}";
        return new Card(newName, a.Cost, a.Type, a.Value + valueBoost, null, a.Ability, newRarity);
    }

    public static int GetCraftCost(CardRarity rarity)
    {
        return rarity switch
        {
            CardRarity.Common => 20,
            CardRarity.Uncommon => 50,
            _ => 0
        };
    }

    public static Card GetRandomCardOfRarity(Random rng, CardRarity rarity)
    {
        List<Card> pool = rarity switch
        {
            CardRarity.Common    => [Strike, Defend, QuickSlash, Heal, HeavyBlow, IronGuard, ArcaneTap],
            CardRarity.Uncommon  => [FlameStrike, PoisonDart, VampiricSlash, TwinStrike, ShieldBash, WeakeningCurse, Fortification, BattleTrance, Meditation, IronHeart, BloodRune],
            CardRarity.Rare      => [DragonBreath, SoulDrain, ThornWall, ArcaneBarrage, ManaCrystal, ManaSpring, ManaWell, LuckyCharm],
            CardRarity.Legendary => [Oblivion, DemonicPact, DragonHeart, ArcaneCore],
            _                    => [Strike]
        };
        return pool[rng.Next(pool.Count)].Clone();
    }
}
