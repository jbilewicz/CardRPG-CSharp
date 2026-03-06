namespace CardRPG.Core.Models;

public class PlayerStats
{
    public long TotalDamageDealt { get; set; } = 0;
    public int EnemiesKilled { get; set; } = 0;
    public int ElitesKilled { get; set; } = 0;
    public int BossesKilled { get; set; } = 0;
    public int RealmsCompleted { get; set; } = 0;
    public int HighestCombo { get; set; } = 0;
    public int HighestHit { get; set; } = 0;
    public long TotalGoldEarned { get; set; } = 0;
    public int CardsEnchanted { get; set; } = 0;
}
