namespace CardRPG.Core.Models;

public class Quest
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string TargetType { get; set; }
    public int TargetCount { get; set; }
    public int RewardGold { get; set; }
    public int RewardXp { get; set; }
    public Card? RewardCard { get; set; }
    public int MinLevel { get; set; }

    public Quest(string id, string name, string desc, string targetType, int targetCount,
                 int rewardGold, int rewardXp, Card? rewardCard = null, int minLevel = 1)
    {
        Id = id;
        Name = name;
        Description = desc;
        TargetType = targetType;
        TargetCount = targetCount;
        RewardGold = rewardGold;
        RewardXp = rewardXp;
        RewardCard = rewardCard;
        MinLevel = minLevel;
    }

    public bool IsComplete(Player player)
    {
        int progress = GetProgress(player);
        return progress >= TargetCount;
    }

    public int GetProgress(Player player)
    {
        if (player.QuestProgress.TryGetValue(Id, out int val))
            return Math.Min(val, TargetCount);
        return 0;
    }

    public static List<Quest> GetAllQuests() => new()
    {
        new("kill_goblins", "Goblin Menace", "Slay 3 Goblin-type enemies.", "kill_goblin", 3, 25, 40,
            CardLibrary.PoisonDart.Clone()),
        new("kill_slimes", "Slime Cleanup", "Kill 5 Slimes.", "kill_slime", 5, 20, 30),
        new("kill_wolves", "Wolf Hunt", "Defeat 4 Wolves.", "kill_wolf", 4, 30, 50),
        new("kill_boss_any", "Boss Bounty", "Defeat 2 bosses.", "kill_boss", 2, 60, 100,
            CardLibrary.ShieldBash.Clone(), 2),
        new("kill_spiders", "Spider Nest", "Exterminate 5 Spiders.", "kill_spider", 5, 35, 60, null, 2),
        new("kill_orcs", "Orc Warband", "Defeat 3 Orc warriors.", "kill_orc", 3, 50, 80,
            CardLibrary.FlameStrike.Clone(), 3),
        new("kill_undead", "Undead Rising", "Slay 6 Undead creatures.", "kill_undead", 6, 45, 70, null, 3),
        new("arena_3", "Arena Rookie", "Clear 3 arena waves.", "arena_waves", 3, 40, 60,
            CardLibrary.TwinStrike.Clone(), 2),
        new("arena_10", "Arena Veteran", "Clear 10 arena waves.", "arena_waves", 10, 100, 200,
            CardLibrary.VampiricSlash.Clone(), 4),
        new("kill_dragons", "Dragon Slayer", "Defeat 2 Dragons.", "kill_dragon", 2, 80, 150,
            CardLibrary.SoulDrain.Clone(), 5),
        new("kill_demons", "Demon Hunter", "Defeat 3 Demons.", "kill_demon", 3, 100, 200,
            CardLibrary.DemonicPact.Clone(), 6),
        new("boss_hunter", "Elite Hunter", "Defeat 5 bosses total.", "kill_boss", 5, 120, 250, null, 4),
    };

    public static void TrackKill(Player player, string enemyName, bool isBoss)
    {
        string lower = enemyName.ToLower();

        foreach (var qid in player.ActiveQuestIds.ToList())
        {
            var q = GetAllQuests().FirstOrDefault(x => x.Id == qid);
            if (q == null) continue;

            bool match = q.TargetType switch
            {
                "kill_goblin" => lower.Contains("goblin"),
                "kill_slime" => lower.Contains("slime"),
                "kill_wolf" => lower.Contains("wolf"),
                "kill_spider" => lower.Contains("spider"),
                "kill_orc" => lower.Contains("orc") || lower.Contains("warlord"),
                "kill_undead" => lower.Contains("skeleton") || lower.Contains("lich") || lower.Contains("wraith") || lower.Contains("zombie"),
                "kill_dragon" => lower.Contains("dragon") || lower.Contains("drake") || lower.Contains("wyrm"),
                "kill_demon" => lower.Contains("demon") || lower.Contains("void") || lower.Contains("abyss"),
                "kill_boss" => isBoss,
                _ => false,
            };

            if (match)
            {
                if (!player.QuestProgress.ContainsKey(qid))
                    player.QuestProgress[qid] = 0;
                player.QuestProgress[qid]++;
            }
        }
    }

    public static void TrackArena(Player player, int wavesCleared)
    {
        foreach (var qid in player.ActiveQuestIds.ToList())
        {
            var q = GetAllQuests().FirstOrDefault(x => x.Id == qid);
            if (q == null || q.TargetType != "arena_waves") continue;

            if (!player.QuestProgress.ContainsKey(qid))
                player.QuestProgress[qid] = 0;
            player.QuestProgress[qid] = Math.Max(player.QuestProgress[qid], wavesCleared);
        }
    }
}
