namespace CardRPG.Models;

public class Player : Entity
{
    //  STATS 
    public int Strength { get; set; } = 10;
    public int Agility { get; set; } = 10;
    public int Intelligence { get; set; } = 10;

    //  RESOURCES 
    public int MaxMana { get; set; } = 3;
    public int CurrentMana { get; set; }
    public int Gold { get; set; } = 0;
    public int Armor { get; set; } = 0;

    //  DECK 
    public List<Card> MasterDeck { get; set; } = new List<Card>();
    public List<Card> Hand { get; set; } = new List<Card>();

    public List<Item> Inventory {get; set;} = new List<Item>();
    public Weapon? EquippedWeapon{get;set;}
    public Player(string name) : base(name, 100)
    {
        CurrentMana = MaxMana;
        Inventory.Add(new Consumable("Small Potion", 20, 10));
    }

    public Player():base("Unknown", 100){}
    
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
        return Intelligence * 1.0; //each intelligence point equals 1% chance of critical damage
    }

    public int GetTotalDamage()
    {
        int weaponDmg = EquippedWeapon != null? EquippedWeapon.DamageBonus : 0;
        return Strength + weaponDmg;
    }
}