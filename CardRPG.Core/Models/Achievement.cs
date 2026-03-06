namespace CardRPG.Core.Models;

public class Achievement
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int RewardGold { get; set; }
    public int RewardXp { get; set; }

    public Achievement(string id, string name, string description, string icon, int rewardGold, int rewardXp)
    {
        Id = id;
        Name = name;
        Description = description;
        Icon = icon;
        RewardGold = rewardGold;
        RewardXp = rewardXp;
    }

    public bool CheckCompletion(Player player)
    {
        return Id switch
        {
            "kill_10" => player.TotalKills >= 10,
            "kill_50" => player.TotalKills >= 50,
            "kill_100" => player.TotalKills >= 100,
            "kill_500" => player.TotalKills >= 500,
            "boss_1" => player.TotalBossKills >= 1,
            "boss_5" => player.TotalBossKills >= 5,
            "boss_10" => player.TotalBossKills >= 10,
            "cards_10" => player.TotalCardsCollected >= 10,
            "cards_20" => player.TotalCardsCollected >= 20,
            "cards_50" => player.TotalCardsCollected >= 50,
            "gold_500" => player.TotalGoldEarned >= 500,
            "gold_2000" => player.TotalGoldEarned >= 2000,
            "level_5" => player.Level >= 5,
            "level_10" => player.Level >= 10,
            "level_20" => player.Level >= 20,
            "realm_3" => player.RealmsCompleted >= 3,
            "realm_5" => player.RealmsCompleted >= 5,
            "realm_10" => player.RealmsCompleted >= 10,
            "arena_5" => player.ArenaWavesCleared >= 5,
            "arena_20" => player.ArenaWavesCleared >= 20,
            "arena_50" => player.ArenaWavesCleared >= 50,
            "deck_15" => player.MasterDeck.Count >= 15,
            _ => false
        };
    }

    public static List<Achievement> GetAll() => new()
    {
        new("kill_10", "First Blood", "Defeat 10 enemies", "Sword", 20, 30),
        new("kill_50", "Warrior", "Defeat 50 enemies", "Sword", 50, 80),
        new("kill_100", "Slayer", "Defeat 100 enemies", "Sword", 100, 200),
        new("kill_500", "Legend", "Defeat 500 enemies", "Sword", 300, 500),
        new("boss_1", "Boss Hunter", "Defeat your first boss", "Crown", 30, 50),
        new("boss_5", "Boss Slayer", "Defeat 5 bosses", "Crown", 80, 150),
        new("boss_10", "Kingslayer", "Defeat 10 bosses", "Crown", 200, 400),
        new("cards_10", "Collector", "Collect 10 cards total", "Card", 25, 40),
        new("cards_20", "Hoarder", "Collect 20 cards total", "Card", 50, 100),
        new("cards_50", "Archivist", "Collect 50 cards total", "Card", 150, 300),
        new("gold_500", "Rich", "Earn 500 gold total", "Gold", 50, 60),
        new("gold_2000", "Wealthy", "Earn 2000 gold total", "Gold", 150, 200),
        new("level_5", "Apprentice", "Reach level 5", "Star", 30, 50),
        new("level_10", "Veteran", "Reach level 10", "Star", 80, 150),
        new("level_20", "Master", "Reach level 20", "Star", 200, 400),
        new("realm_3", "Explorer", "Complete 3 realms", "Map", 50, 80),
        new("realm_5", "Adventurer", "Complete 5 realms", "Map", 100, 200),
        new("realm_10", "Conqueror", "Complete all 10 realms", "Map", 500, 1000),
        new("arena_5", "Gladiator", "Clear 5 arena waves", "Shield", 40, 60),
        new("arena_20", "Champion", "Clear 20 arena waves", "Shield", 120, 250),
        new("arena_50", "Arena Master", "Clear 50 arena waves", "Shield", 300, 600),
        new("deck_15", "Deck Builder", "Have 15+ cards in deck", "Card", 40, 60),
    };
}
