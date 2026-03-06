namespace CardRPG.Core.Models;

public class Player : Entity
{
    public int Strength { get; set; } = 10;
    public int Agility { get; set; } = 10;
    public int Intelligence { get; set; } = 10;

    public int Level { get; set; } = 1;
    public int XP { get; set; } = 0;
    public int MaxRealmUnlocked { get; set; } = 1;
    public int TalentPoints { get; set; } = 0;

    public int MaxMana { get; set; } = 3;
    public int CurrentMana { get; set; }
    public int Gold { get; set; } = 0;
    public int Armor { get; set; } = 0;

    public List<Card> MasterDeck { get; set; } = new List<Card>();
    public List<Card> Hand { get; set; } = new List<Card>();

    public List<Item> Inventory { get; set; } = new List<Item>();
    public Weapon? EquippedWeapon { get; set; }

    public List<string> CompletedAchievements { get; set; } = new();
    public int TotalKills { get; set; } = 0;
    public int TotalBossKills { get; set; } = 0;
    public int TotalGoldEarned { get; set; } = 0;
    public int TotalCardsCollected { get; set; } = 0;
    public int RealmsCompleted { get; set; } = 0;
    public int ArenaWavesCleared { get; set; } = 0;

    public List<string> ActiveQuestIds { get; set; } = new();
    public List<string> CompletedQuestIds { get; set; } = new();
    public Dictionary<string, int> QuestProgress { get; set; } = new();

    public PlayerClass Class { get; set; } = PlayerClass.Warrior;
    public List<TalentNode> Talents { get; set; } = TalentNode.CreateDefaults();
    public PlayerStats Stats { get; set; } = new();

    public Player(string name) : base(name, 100)
    {
        CurrentMana = MaxMana;
        Inventory.Add(new Consumable("Small Potion", 20, 10));
    }

    public Player() : base("Unknown", 100) { }

    public int XpForNextLevel => Level * 120 + (Level * Level * 10);

    public bool GainXp(int amount)
    {
        XP += amount;
        bool leveledUp = false;
        while (XP >= XpForNextLevel)
        {
            XP -= XpForNextLevel;
            Level++;
            TalentPoints += 2;
            MaxHp += 10;
            CurrentHp += 10;
            MaxMana += (Level % 3 == 0) ? 1 : 0;
            leveledUp = true;
        }
        return leveledUp;
    }

    public override void TakeDamage(int damage)
    {
        if (Armor > 0)
        {
            if (damage >= Armor)
            {
                damage -= Armor;
                Armor = 0;
            }
            else
            {
                Armor -= damage;
                damage = 0;
            }
        }
        base.TakeDamage(damage);
    }

    public double CalculateCritChance()
    {
        int bonusCrit = GetPassiveBonus("CritChance");
        return Intelligence * 1.0 + bonusCrit;
    }

    public int GetTotalDamage()
    {
        int weaponDmg = EquippedWeapon != null ? EquippedWeapon.DamageBonus : 0;
        int passiveDmg = GetPassiveBonus("Damage");
        return Strength + weaponDmg + passiveDmg;
    }

    public int GetPassiveBonus(string effect)
    {
        return MasterDeck.Where(c => c.IsPassive && c.PassiveEffect == effect).Sum(c => c.PassiveValue);
    }

    public int GetPassiveMaxHpBonus() => GetPassiveBonus("MaxHP");
    public int GetPassiveManaBonus() => GetPassiveBonus("MaxMana");
    public int GetEffectiveMaxHp() => MaxHp + GetPassiveMaxHpBonus();
    public int GetEffectiveMaxMana() => MaxMana + GetPassiveManaBonus();
}