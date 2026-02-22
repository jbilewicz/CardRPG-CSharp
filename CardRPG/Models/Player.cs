namespace CardRPG.Models;

public class Player
{
    public string Name {get; set;}
    //atributes
    public int Strength {get; set;} = 10 ; //increase DMG
    public int Agility {get; set;} = 10; //dodges and defense against attack
    public int Intelligence{get;set;} = 10; //chance for a critical attack

    //resources
    public int MaxHp {get;set;} = 100;
    public int CurrentHp {get;set;}
    public int MaxMana{get;set;} = 3;
    public int CurrentMana{get; set;}
    public int Gold {get; set;} = 0;
    public int Armor{get;set;} = 0;

    public Player(string name)
    {
        Name = name;
        CurrentHp = MaxHp;
        CurrentMana = MaxMana;
    }  

    public void TakeDamage(int damage)
    {
        if(Armor > 0)
        {
            if (damage >= Armor)
            {
                damage-=Armor;
                Armor = 0;
            }
            else
            {
                Armor -= damage;
                damage = 0;
            }
        }
        CurrentHp -= damage;
        if (CurrentHp<0) CurrentHp = 0;
    }
    
    public double CalculateCritChance()
    {
        return Intelligence * 1.0; //each intelligence point equals 1% chance of critical damage
    }
}