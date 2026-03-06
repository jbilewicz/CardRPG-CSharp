namespace CardRPG.Core.Models;

public enum TalentTree
{
    Warrior,
    Rogue,
    Mage
}

public class TalentNode
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public TalentTree Tree { get; set; }
    public int Tier { get; set; }
    public int CurrentRanks { get; set; } = 0;
    public int MaxRanks { get; set; } = 1;
    public string Effect { get; set; } = "";
    public int ValuePerRank { get; set; } = 0;

    public TalentNode() { }

    public TalentNode(string name, string description, TalentTree tree, int tier, int maxRanks, string effect = "", int valuePerRank = 0)
    {
        Name = name;
        Description = description;
        Tree = tree;
        Tier = tier;
        MaxRanks = maxRanks;
        Effect = effect;
        ValuePerRank = valuePerRank;
    }

    public static List<TalentNode> CreateDefaults() => new()
    {
        // --- Warrior ---
        new("Brute Force",     "+2 Damage per rank",       TalentTree.Warrior, 1, 3, "Damage",   2),
        new("Iron Skin",       "+5 Max HP per rank",       TalentTree.Warrior, 1, 3, "MaxHP",    5),
        new("Battle Hardened", "+2 Armor per rank",        TalentTree.Warrior, 2, 2, "Armor",    2),
        new("Toughness",       "+10 Max HP per rank",      TalentTree.Warrior, 2, 2, "MaxHP",   10),
        new("Warlord",         "+5 Damage, +10 Max HP",    TalentTree.Warrior, 3, 1, "Damage",   5),

        // --- Rogue ---
        new("Swift Strikes",   "+1% Crit per rank",        TalentTree.Rogue,   1, 3, "CritChance", 1),
        new("Pickpocket",      "+10% Gold per rank",       TalentTree.Rogue,   1, 2, "Gold",       10),
        new("Shadow Step",     "+2% Crit per rank",        TalentTree.Rogue,   2, 2, "CritChance", 2),
        new("Cheap Shot",      "+2 Damage per rank",       TalentTree.Rogue,   2, 2, "Damage",     2),
        new("Assassinate",     "+10% Crit chance",         TalentTree.Rogue,   3, 1, "CritChance",10),

        // --- Mage ---
        new("Arcane Mind",     "+1 Crit per rank",         TalentTree.Mage,    1, 3, "CritChance", 1),
        new("Mana Surge",      "+1 Max Mana per rank",     TalentTree.Mage,    1, 2, "MaxMana",    1),
        new("Spell Power",     "+2 Intelligence per rank", TalentTree.Mage,    2, 2, "Damage",     2),
        new("Meditation",      "+5 Max HP per rank",       TalentTree.Mage,    2, 2, "MaxHP",      5),
        new("Archmage",        "+2 Max Mana, +5 Crit",     TalentTree.Mage,    3, 1, "MaxMana",    2),
    };
}
