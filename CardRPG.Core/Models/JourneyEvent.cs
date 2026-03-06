namespace CardRPG.Core.Models;

public enum JourneyEventType
{
    Treasure,
    Fountain,
    Trap,
    Shrine,
    Wanderer,
    Merchant
}

public class JourneyEvent
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string ChoiceA { get; set; } = "";
    public string ChoiceB { get; set; } = "";
    public JourneyEventType Type { get; set; }

    private readonly Func<Player, Random, (string, int, int, Card?)> _resolveA;
    private readonly Func<Player, Random, (string, int, int, Card?)> _resolveB;

    public JourneyEvent(
        string title, string description,
        string choiceA, string choiceB,
        JourneyEventType type,
        Func<Player, Random, (string, int, int, Card?)> resolveA,
        Func<Player, Random, (string, int, int, Card?)> resolveB)
    {
        Title = title;
        Description = description;
        ChoiceA = choiceA;
        ChoiceB = choiceB;
        Type = type;
        _resolveA = resolveA;
        _resolveB = resolveB;
    }

    public (string message, int gold, int hp, Card? card) ResolveChoiceA(Player player, Random rng)
        => _resolveA(player, rng);

    public (string message, int gold, int hp, Card? card) ResolveChoiceB(Player player, Random rng)
        => _resolveB(player, rng);

    // --- Static factory methods for common events ---

    public static JourneyEvent CreateFountain() => new(
        "Mysterious Fountain",
        "You stumble upon a shimmering fountain. Its waters glow faintly. Do you drink or fill a flask?",
        "Drink from the fountain",
        "Fill a flask (Potion)",
        JourneyEventType.Fountain,
        (player, rng) =>
        {
            int roll = rng.Next(100);
            if (roll < 60)
                return ("The water restores your health!", 0, 30, null);
            else if (roll < 85)
                return ("The water surges with power — you feel stronger!", 0, 15, null);
            else
                return ("The water was cursed! You feel weakened.", 0, -20, null);
        },
        (player, rng) => ("You fill your flask with the glowing water. Small Potion obtained!", 0, 0, null)
    );

    public static JourneyEvent CreateTreasure() => new(
        "Abandoned Chest",
        "A dusty chest sits in a corner. It could contain riches — or a trap.",
        "Open the chest",
        "Leave it alone",
        JourneyEventType.Treasure,
        (player, rng) =>
        {
            int roll = rng.Next(100);
            if (roll < 70)
            {
                int gold = rng.Next(20, 60);
                return ($"You found {gold} gold inside!", gold, 0, null);
            }
            else
                return ("The chest was trapped! A blade swings at you.", 0, -25, null);
        },
        (player, rng) => ("You walk away carefully. Better safe than sorry.", 0, 0, null)
    );

    public static JourneyEvent CreateTrap() => new(
        "Suspicious Floor",
        "The floor ahead looks suspicious. You could try to disarm it or just dash through.",
        "Disarm the trap carefully",
        "Sprint through!",
        JourneyEventType.Trap,
        (player, rng) =>
        {
            int roll = rng.Next(100);
            if (roll < 65)
                return ("You carefully disarm the mechanism — no harm done.", 0, 0, null);
            else
                return ("The trap triggers anyway! Spikes graze you.", 0, -15, null);
        },
        (player, rng) =>
        {
            int roll = rng.Next(100);
            if (roll < 50)
                return ("You sprint through successfully — barely!", 0, 0, null);
            else
                return ("You weren't fast enough. The trap caught you.", 0, -30, null);
        }
    );

    public static JourneyEvent CreateShrine() => new(
        "Ancient Shrine",
        "A weathered shrine stands before you, adorned with ancient symbols. You sense power within.",
        "Make an offering (10 Gold)",
        "Pray without offering",
        JourneyEventType.Shrine,
        (player, rng) =>
        {
            if (player.Gold < 10)
                return ("You don't have enough gold to make an offering.", 0, 0, null);
            int roll = rng.Next(100);
            if (roll < 80)
                return ("The shrine blesses you with renewed vitality!", -10, 40, null);
            else
                return ("The shrine pulses with energy, granting you a vision.", -10, 20, null);
        },
        (player, rng) =>
        {
            int roll = rng.Next(100);
            if (roll < 40)
                return ("The shrine accepts your prayer — a faint warmth washes over you.", 0, 15, null);
            else
                return ("Nothing happens. The gods are silent today.", 0, 0, null);
        }
    );

    public static JourneyEvent CreateWanderer() => new(
        "Travelling Merchant",
        "A hooded wanderer blocks your path. They offer to trade if you're willing.",
        "Trade 15 Gold for info (+5 HP)",
        "Ignore and move on",
        JourneyEventType.Wanderer,
        (player, rng) =>
        {
            if (player.Gold < 15)
                return ("You don't have enough gold.", 0, 0, null);
            return ("The wanderer shares healing herbs with you.", -15, 20, null);
        },
        (player, rng) => ("You brush past the wanderer without a word.", 0, 0, null)
    );

    public static List<JourneyEvent> CreateRandomPool() =>
    [
        CreateFountain(),
        CreateTreasure(),
        CreateTrap(),
        CreateShrine(),
        CreateWanderer(),
    ];
}
